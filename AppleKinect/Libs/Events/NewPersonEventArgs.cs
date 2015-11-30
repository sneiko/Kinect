using System;
using GestureDetector.DataSources;

namespace GestureDetector.Events
{
    /// <summary>
    /// A new person was detected, passive
    /// </summary>
    public class NewPersonEventArgs: EventArgs
    {
        /// <summary>
        /// The new person
        /// </summary>
        public Person Person { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">The new person</param>
        public NewPersonEventArgs(Person p)
        {
            Person = p;
        }

    }
}
