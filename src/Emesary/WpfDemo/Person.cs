using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfDemo
{
    /// <summary>
    /// Represents Person - for use in sample data.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Full name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Free format date of birth.
        /// </summary>
        public string DateOfBirth { get; set; }

        /// <summary>
        /// associated SSN or NI number, or other healthcare provider - generally
        /// used to uniquely identify an individual.
        /// </summary>
        public string HealthcareNumber { get; set; }
    }
}
