using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct ContactDetails
    {
        public static ContactDetails Default = default(ContactDetails);

        public int OwnerID;
        public string Address;
        public string LandNumber;
        public string IPAddress;
        public string E_Mail;
        public string MobileNumber;

        public static ContactDetails Create()
        {
            return new ContactDetails()
            {
                OwnerID = 0,
                Address = string.Empty,
                LandNumber = string.Empty,
                IPAddress = string.Empty,
                E_Mail = string.Empty,
                MobileNumber = string.Empty
            };
        }
    }
}
