using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GestureDetector.Events;
using GestureDetector.Exceptions;
using GestureDetector.Tools;
using Microsoft.Kinect;

namespace GestureDetector.DataSources
{
    
    /// <summary>
    /// Mapper for the KinectSensor. 
    /// Implements person recognition, login and activity.
    /// 
    /// Restrictions:
    /// - If the physical device is moving, persons currenty aren't tracked</summary>
    public class Device
    {
        /// <summary>
        /// How long we keep invisible persons in cache [milliseconds].</summary>
        private const int CacheMercyTime = 5000;

        private KinectSensor KinectDevice;

        /// <summary>
        /// We keep track of the sensors movement. If its moving, there wont be any gesture recognition.</summary>
        private Vector4 _lastAcceleration;

        /// <summary>
        /// List of persons which are currently tracked</summary>
        private List<Person> _visiblePersons;

        /// <summary>
        /// Expiration Candidates - All persons which currently dont have a skeleton
        /// We keep them for 5 seconds to prevent glitches in gesture recognition.</summary>
        private Dictionary<long, Person> _expirationCandidates;

        private bool _nearMode;
        private bool _seated;
        private Filter _filter = new Filter();

        private const double MinMatchDistance = 0.5;

        private const double AccelerationDiff = 0.1;

        /// <summary>
        /// Initialization of the kinect wrapper and the physical device. 
        /// It handles frame events, creates new persons and decides who's currently active.</summary>
        public Device()
        {
            // Receive KinectSensor instance from physical device
            KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            if (KinectDevice == null)
            {
                throw new DeviceErrorException("No connected device");
            }
            Initialize();
        }

        /// <summary>
        /// Register relevant device events and begin streaming</summary>
        private void Initialize()
        {
            _lastAcceleration = new Vector4();
            _visiblePersons = new List<Person>();
            _expirationCandidates = new Dictionary<long, Person>();
            KinectDevice.ColorStream.Disable();
            PersonsToTrack = 2;
            KinectDevice.SkeletonStream.AppChoosesSkeletons = true;
        }

        /// <summary>
        /// Geting a specified Sensor
        /// </summary>
        /// <param name="uniqueId">The serial number of the Kinect</param>
        public Device(string uniqueId) // get a specified Kinect by its ID
        {
            foreach (KinectSensor ks in KinectSensor.KinectSensors)
            {
                if (ks.UniqueKinectId == uniqueId)
                {
                    KinectDevice = ks;
                    Initialize();
                }
            }
            if (KinectDevice == null)
            {
                throw new DeviceErrorException("No connected device");
            }
        }

        /// <summary>
        /// The status of the Kinect Sensor
        /// </summary>
        public KinectStatus Status { get { return KinectDevice.Status; } }

        /// <summary>
        /// The original KinectSenor
        /// <remarks>If you change some settings, the recognition may not detect any persons</remarks>
        /// </summary>
        public KinectSensor Sensor { get { return KinectDevice; } }

        /// <summary>
        /// Start receiving skeletons.</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            if (!KinectDevice.SkeletonStream.IsEnabled)
            {
                KinectDevice.SkeletonStream.Enable(_filter.SmoothingParam); // Begin to capture skeletons
                KinectDevice.SkeletonFrameReady += OnNewSkeletons; // Register on any new skeletons
            }
            if (!KinectDevice.IsRunning)
            {
                KinectDevice.Start();
            }
            if (!KinectDevice.IsRunning)
            {
                throw new DeviceErrorException("Device did not start") { Status = KinectDevice.Status } ;
            }
        }

        /// <summary>
        /// Start receiving skeletons using a custom Filter</summary>
        /// <param name="filter">A Filter</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start(Filter filter)
        {
            _filter = filter;
            if (!KinectDevice.SkeletonStream.IsEnabled)
            {
                KinectDevice.SkeletonStream.Enable(_filter.SmoothingParam); // Begin to capture skeletons
                KinectDevice.SkeletonFrameReady += OnNewSkeletons; // Register on any new skeletons
            }
            if (!KinectDevice.IsRunning)
            {
                KinectDevice.Start();
            }
            if (!KinectDevice.IsRunning)
            {
                throw new DeviceErrorException("Device did not start") { Status = KinectDevice.Status };
            }
        }

        /// <summary>
        /// Stop receiving skeletons.</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if (KinectDevice.SkeletonStream.IsEnabled)
            {
                KinectDevice.SkeletonStream.Disable();
                KinectDevice.SkeletonFrameReady -= OnNewSkeletons;
            }
            if (KinectDevice.IsRunning)
            {
                KinectDevice.Stop();
            }
            if (KinectDevice.IsRunning)
            {
                throw new DeviceErrorException("Device did not stop") { Status = KinectDevice.Status };
            }
        }

        /// <summary>
        /// Enables the Near-Mode
        /// </summary>
        public bool NearMode
        {
            get { return _nearMode; }
            set
            {
                _nearMode = value;
                KinectDevice.DepthStream.Range = value ? DepthRange.Near : DepthRange.Default;
                KinectDevice.SkeletonStream.EnableTrackingInNearRange = value;
            }
        }

        /// <summary>
        /// The elevation angle of the Kinect
        /// </summary>
        public int ElevationAngle { get { return KinectDevice.ElevationAngle; } set
        {
            if (value < KinectDevice.MinElevationAngle || value > KinectDevice.MaxElevationAngle)
            {
                throw new DeviceErrorException("Invalid Angle");
            }
            try
            {
                KinectDevice.ElevationAngle = value;
            }
            catch (Exception)
            {
                throw new DeviceErrorException("Tilt Motor Problem");
            }
        } }

        /// <summary>
        /// Enables Seated Skeleton-Mode
        /// </summary>
        public bool Seated
        {
            get { return _seated; }
            set
            {
                _seated = value;
                KinectDevice.SkeletonStream.TrackingMode = value ? SkeletonTrackingMode.Seated : SkeletonTrackingMode.Default;
            }
        }

        /// <summary>
        /// Set a custom filter
        /// </summary>
        /// <remarks>
        /// Use preferable before start
        /// If used after Stert(), there will be a Pause in recognition
        /// </remarks>
        public void SetFilter(Filter filter)
        {
            _filter = filter;
            if (KinectDevice.SkeletonStream.IsEnabled)
            {
                KinectDevice.SkeletonStream.Disable();
                KinectDevice.SkeletonStream.Enable(filter.SmoothingParam);
            }
        }

        /// <summary>
        /// Disopse device resources.</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            Stop();
            KinectDevice.Dispose();
        }

        /// <summary>
        /// Should always return the only currently active person. It would theoretically
        /// be possible to serve multiple active persons, but its not implemented.</summary>
        /// <returns>
        /// Returns the currently active person. If no person is active, null is returned.</returns>
        public IEnumerable<Person> GetActivePersons()
        {
            return _visiblePersons.FindAll(x => x.Active);
        }

        /// <summary>
        /// Get all persons tracked by the Kinect.</summary>
        /// <returns>
        /// List of tracked persons</returns>
        public List<Person> GetAllPersons()
        {
            return _visiblePersons;
        }

        /// <summary>
        /// Event for receiving new skeletons. Its called on every frame delivered
        /// by the KinectSensor (every 30ms) and decides if a frame is valid or will
        /// be ignored. If a frame is valid, we read all tracked skeletons from it
        /// and smooth it. After that, the skeletons are analyzed (handleSkeletons).</summary>
        /// <param name="source">
        /// The source is probably the KinectSensor</param>
        /// <param name="e">
        /// Resource to get the current frame from.</param>
        protected void OnNewSkeletons(object source, SkeletonFrameReadyEventArgs e)
        {
            double diff = GetAccelerationDiff();
            bool b = (diff > AccelerationDiff || diff < -AccelerationDiff) && Accelerated != null;
            if (b) 
            {
                // Fire event for indicating unstable device
                Accelerated(this, new AccelerationEventArgs(diff));
            }
            else
            {
                SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    // Copy actually tracked skeletons to a list which is easier to work with.
                    List<SmothendSkeleton> skeletonList = (from ske in skeletons where ske.TrackingState == SkeletonTrackingState.Tracked || ske.TrackingState == SkeletonTrackingState.PositionOnly select new SmothendSkeleton(ske, skeletonFrame.Timestamp)).ToList();
                    skeletonFrame.Dispose();
                    HandleNewSkeletons(skeletonList);
                }
            }
        }

        /// <summary>
        /// Analyze skeletons and assign them to existing persons. This is done
        /// with a naive matching technique:
        ///   Compare every new skeletons hip center with every cached persons 
        ///   last skeletons hip center.
        /// After a person got a new skeletons, his timestamp in the cache is renewed.</summary>
        /// <param name="skeletonsToMatch">
        /// List of smoothend skeletons</param>
        protected void HandleNewSkeletons(List<SmothendSkeleton> skeletonsToMatch)
        {
            // Remove persons older than 5 seconds from dictionary
            long allowedAge = CurrentMillis.Millis - CacheMercyTime;
            List<KeyValuePair<long, Person>> tempList = new List<KeyValuePair<long,Person>>();
            tempList.AddRange(_expirationCandidates.Where(x => x.Key > allowedAge).ToList());
            foreach (KeyValuePair<long, Person> item in tempList)
            {
                // avoiding memory leak
                item.Value.OnWave -= PersonWaved;
                _expirationCandidates.Remove(item.Key);
            }

            /*
             * There are 3 possibilities to describe the current status
             * - Every person had a skeleton. We just need to assign new ones.
             * - There are less skeletons than persons. At least one person doesnt exist 
             *   anymore and has to be deleted (it still exists in the cache).
             * - There are more skeletons than persons. Maybe we have to rehabilitate 
             *   a cached person or to create a new one.
             */
            Match bestMatch = new Match();

            /*
             * A person went out of sight
             */
            if (skeletonsToMatch.Count < _visiblePersons.Count) 
            {
                List<Person> personList = new List<Person>(); // copy currently tracked persons for the matching algorithm
                personList.AddRange(_visiblePersons);
                foreach (SmothendSkeleton s in skeletonsToMatch) // search best match for every SKELETON
                {
                    bestMatch.Value = double.MaxValue;
                    foreach (Person p in personList)
                    {
                        double v = p.Match(s);
                        if (v < bestMatch.Value)
                        {
                            bestMatch.Value = v;
                            bestMatch.Person = p;
                            bestMatch.Skeleton = s;
                        }
                    }
                    bestMatch.Person.AddSkeleton(bestMatch.Skeleton); // Assign new skeleton
                    personList.Remove(bestMatch.Person);
                }

                // Delete remaining persons since they don't have a skeleton anymore (they still exists in cache)
                foreach (Person p in personList)
                {
                    _visiblePersons.Remove(p);
                    _expirationCandidates.Add(CurrentMillis.Millis, p); // Add person to cache
                    if (PersonLost != null) 
                    {
                        PersonLost(this, new PersonDisposedEventArgs(p));
                    }
                }
            }

            /*
             * A person came in sight or there were no changes
             */
            else
            {
                List<Person> personList = new List<Person>(); // copy currently tracked persons for the matching algorithm
                personList.AddRange(_visiblePersons);
                foreach (Person p in personList) // search best match for every PERSON
                {
                    bestMatch.Value = double.MaxValue;
                    double v;
                    foreach (SmothendSkeleton s in skeletonsToMatch)
                    {
                        v = p.Match(s);
                        if (v < bestMatch.Value)
                        {
                            bestMatch.Value = v;
                            bestMatch.Person = p;
                            bestMatch.Skeleton = s;
                        }
                    }
                    bestMatch.Person.AddSkeleton(bestMatch.Skeleton); // assign new skeleton
                    skeletonsToMatch.Remove(bestMatch.Skeleton);
                }

                /*
                 * After we matched the tracked persons, we try to match the 
                 * remaining skeletons to the cached persons.
                 */
                List<SmothendSkeleton> skeletonsToRemove = new List<SmothendSkeleton>();
                foreach (SmothendSkeleton s in skeletonsToMatch)
                {
                    bestMatch.Value = double.MaxValue;
                    double v;
                    foreach (long l in _expirationCandidates.Keys)
                    {
                        Person p = _expirationCandidates[l];
                        v = p.Match(s);
                        if (v < bestMatch.Value)
                        {
                            bestMatch.Value = v;
                            bestMatch.Person = p;
                            bestMatch.Skeleton = s;
                            bestMatch.Timestamp = l;
                        }
                    }
                    if (bestMatch.Value < MinMatchDistance) // match valid
                    {
                        // There was a match. Person will be rehabilitated and tracked again.
                        _visiblePersons.Add(bestMatch.Person);
                        RegisterWave(bestMatch.Person);
                        bestMatch.Person.AddSkeleton(bestMatch.Skeleton); // Assign the new Skeleton
                        _expirationCandidates.Remove(bestMatch.Timestamp);
                        skeletonsToRemove.Add(bestMatch.Skeleton);
                    }
                }
                foreach (SmothendSkeleton ske in skeletonsToRemove)
                {
                    skeletonsToMatch.Remove(ske);
                }

                /*
                 * We didn't find neighter a match for tracked persons nor for cached persons.
                 * -> We will create new persons for all unmatched skeletons.
                 */
                foreach (SmothendSkeleton s in skeletonsToMatch)
                {
                    Person p = new Person(this);
                    p.AddSkeleton(s);
                    _visiblePersons.Add(p);
                    RegisterWave(p);
                    if (NewPerson != null)
                    {
                        NewPerson(this, new NewPersonEventArgs(p));
                    }
                }
            }
            ChooseSkeletonsToTrack();
        }

        private void ChooseSkeletonsToTrack()
        {
            if (DefaultTracking)
            {
                KinectDevice.SkeletonStream.ChooseSkeletons();
            }
            else
            {
                var act = GetActivePersons().ToList();
                Random r = new Random();
                switch (act.Count)
                {
                    case 0:
                        FindCandidates(new List<Person>());
                        break;
                    case 1: // track active User
                        if(PersonsToTrack == 1)
                            KinectDevice.SkeletonStream.ChooseSkeletons(act.First().TrackingId);
                        else
                            FindCandidates(new List<Person>{act.First()});
                        break;
                    case 2: // somehow two persons are active, by a glitch or a user action
                        if (PersonsToTrack <= 2)
                            KinectDevice.SkeletonStream.ChooseSkeletons(act.First().TrackingId, act.Last().TrackingId);
                        else
                        {
                            if(act.All(x => x.TrackingState == SkeletonTrackingState.Tracked))
                                FindCandidates(new List<Person>());
                            else
                                KinectDevice.SkeletonStream.ChooseSkeletons(act.First().TrackingId, act.Last().TrackingId);
                        }
                        break;
                    case 3:
                        if (PersonsToTrack <= 3)
                            KinectDevice.SkeletonStream.ChooseSkeletons(
                                act.First(x => x.TrackingState == SkeletonTrackingState.PositionOnly).TrackingId,
                                act.Where(x => x.TrackingState == SkeletonTrackingState.Tracked)
                                   .ElementAt(r.Next(0, 1))
                                   .TrackingId);
                        else
                        {
                            var actTracked = act.Where(x => x.TrackingState == SkeletonTrackingState.Tracked).ToList();
                            if (actTracked.Count == 2)
                                FindCandidates(new List<Person> { act.First(x => x.TrackingState == SkeletonTrackingState.PositionOnly) });
                            else
                                KinectDevice.SkeletonStream.ChooseSkeletons(
                                act.First(x => x.TrackingState == SkeletonTrackingState.PositionOnly).TrackingId,
                                act.Last(x => x.TrackingState == SkeletonTrackingState.PositionOnly).TrackingId);
                        }
                        break;
                    case 4:
                        KinectDevice.SkeletonStream.ChooseSkeletons(
                            act.First(x => x.TrackingState == SkeletonTrackingState.PositionOnly).TrackingId,
                            act.Last(x => x.TrackingState == SkeletonTrackingState.PositionOnly).TrackingId);
                        break;
                    default:
                        throw new DeviceErrorException("Cannot track more than 4 Persons");
                }
            }
        }

        private void FindCandidates(List<Person> persons)
        {
            var act = GetActivePersons().ToList();
            Random r = new Random();
            if (_expirationCandidates.Count(x => x.Value.Active == true) + act.Count < PersonsToTrack)
                // a active person recently disapeared, waiting to come back
            {
                //no disappeared active person
                if (_visiblePersons.Count + persons.Count <= 2 && _visiblePersons.Count > 0)
                {
                    persons = persons.Union(_visiblePersons).ToList();
                }
                else if (_visiblePersons.Count > 2)
                {
                    // too many people to track all
                    var tracked =
                        _visiblePersons.Where(x => x.TrackingState == SkeletonTrackingState.Tracked && !x.Active)
                                       .ToList();
                    foreach (var person in tracked)
                    {
                        if (
                            SkeletonMath.DirectionTo(
                                person.CurrentSkeleton.GetPosition(JointType.HandRight),
                                person.CurrentSkeleton.GetPosition(JointType.ShoulderRight))
                                        .Contains(Direction.Downward) && persons.Count <= 2)
                        {
                            // has the hand over his shoulder
                            persons.Add(person);
                        }
                    }
                    var untracked =
                        _visiblePersons.Where(x => x.TrackingState != SkeletonTrackingState.Tracked)
                                       .ToList();
                    if (untracked.Count + persons.Count <= 2)
                    {
                        // Add the rest
                        persons.AddRange(untracked);
                    }
                    else
                    {
                        // pick random skeletons
                        for (int i = persons.Count; i < 2; i++)
                        {
                            persons.Add(untracked.ElementAt(r.Next(0, untracked.Count - 1)));
                        }
                    }
                }
                else
                {
                    //someting bad happend
                    KinectDevice.SkeletonStream.ChooseSkeletons();
                }
            }
            switch (persons.Count)
            {
                case 1:
                    KinectDevice.SkeletonStream.ChooseSkeletons(persons.First().TrackingId);
                    break;
                case 2:
                    KinectDevice.SkeletonStream.ChooseSkeletons(persons.First().TrackingId, persons.Last().TrackingId);
                    break;
            }
        }

        /// <summary>
        /// Disables the custom tracking of the Skeletons
        /// </summary>
        /// <remarks>
        /// Use when your application doesn't base on the waving login</remarks>
        public bool DefaultTracking { get; set; }

        /// <summary>
        /// Begin detecting if a person is waving.</summary>
        /// <param name="p">Person to monitor</param>
        private void RegisterWave(Person p)
        {
            p.OnWave += PersonWaved;
        }

        /// <summary>
        /// Returns the current acceleration of the physical device.</summary>
        /// <returns>
        /// True if the physical device is moving, false otherwise.</returns>
        private double GetAccelerationDiff()
        {
            double diff = 0; // Difference between last accelerometer readings and actual readings
            diff += (KinectDevice.AccelerometerGetCurrentReading().W - _lastAcceleration.W);
            diff += (KinectDevice.AccelerometerGetCurrentReading().X - _lastAcceleration.X);
            diff += (KinectDevice.AccelerometerGetCurrentReading().Y - _lastAcceleration.Y);
            diff += (KinectDevice.AccelerometerGetCurrentReading().Z - _lastAcceleration.Z);
            _lastAcceleration = KinectDevice.AccelerometerGetCurrentReading();
            return diff;
        }

        /// <summary>
        /// If a person waved, we need to activate her for further gesture recognition.</summary>
        /// <param name="sender">
        /// This device class</param>
        /// <param name="e">
        /// Gesture details</param>
        private void PersonWaved(object sender, GestureEventArgs e)
        {
            Person p = ((Person)sender);
            // Set person active if there isn't another active person
            if (!p.Active && _visiblePersons.Find(x => x.Active==true) == null)
            {
                p.Active = true;
                if (PersonActive != null)
                {
                    PersonActive(this ,new ActivePersonEventArgs(p));
                }
            }
        }

        /// <summary>
        /// Struct for saving details of the matching algoritm.</summary>
        private class Match
        {
            /// <summary>
            /// Distance of the two skeletons hip centers</summary>
            public double Value {get; set; }
            /// <summary>
            /// Person of whom the skeleton was matched</summary>
            public Person Person {get; set; }
            /// <summary>
            /// Compared skeleton</summary>
            public SmothendSkeleton Skeleton {get; set; }
            /// <summary>
            /// The matches timestamp</summary>
            public long Timestamp { get; set; }
        };

        #region Events

        /// <summary>
        /// Called when a person got active</summary>
        public event EventHandler<ActivePersonEventArgs> PersonActive;
        //public delegate void ActivePersonHandler<TEventArgs> (object source, TEventArgs e) where TEventArgs:EventArgs;

        /// <summary>
        /// Called when the device recognized a new person</summary>
        public event EventHandler<NewPersonEventArgs> NewPerson;
        //public delegate void PersonHandler<TEventArgs>(object source, TEventArgs e) where TEventArgs : EventArgs;

        /// <summary>
        /// Called when the Kinect was moving.</summary>
        public event EventHandler<AccelerationEventArgs> Accelerated;

        /// <summary>
        /// Called when a person was lost.</summary>
        public event EventHandler<PersonDisposedEventArgs> PersonLost;

        #endregion

        public int PersonsToTrack { get; set; }
    }
}
