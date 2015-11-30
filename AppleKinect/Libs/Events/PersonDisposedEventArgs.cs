using System;
using GestureDetector.DataSources;

namespace GestureDetector.Events
{
    /// <summary>
    /// A person was disposed
    /// </summary>
    public class PersonDisposedEventArgs: EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="person">The disposed preson</param>
        public PersonDisposedEventArgs(Person person)
        {
            Person = person;
        }

        /// <summary>
        /// The disposed preson
        /// </summary>
        public Person Person { get; private set; }
    }
}
