using System;
using GestureDetector.DataSources;

namespace GestureDetector.Events
{
    /// <summary>
    /// A person became passive
    /// </summary>
    public class PersonPassiveEventArgs:EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="person">The person, that became passive</param>
        public PersonPassiveEventArgs(Person person)
        {
            Person = person;
        }

        /// <summary>
        /// The preson that became passive
        /// </summary>
        public Person Person { get; private set; }
    }
}
