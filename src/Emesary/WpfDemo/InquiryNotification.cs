using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfDemo
{
    class InquiryNotification : Emesary.QueueNotification 
    {
        /// <summary>
        /// The type of the notification. This is to allow for a generic method of inquiring - so probably there
        /// needs to be extra properties to return things once the other types are used.
        /// </summary>
        public enum InquiryType
        {
            CanUse,
            CanDelete,
            CanChange
        }

        /// <summary>
        /// Construct
        /// </summary>
        /// <param name="action">Inquiry Type</param>
        /// <param name="inquriryValue">Value to check</param>
        /// <param name="value">related object, usually the object that is invoking the request.</param>
        public InquiryNotification(InquiryType action, string inquriryValue, object value)
            : base(value)
        {
            Action = action;
            InquiryValue = inquriryValue;
        }
        
        /// <summary>
        /// the required action. This is to allow for different inquiries - such as CanUse, CanDelete etc.
        /// </summary>
        public InquiryType Action { get; set; }

        /// <summary>
        /// the value to check for duplication. Using strings here is easy and will cover most cases.
        /// </summary>
        public string InquiryValue { get; set; }
    }
}
