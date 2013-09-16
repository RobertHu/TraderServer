using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public struct Session
    {
        public static Session InvalidValue = new Session(0);
        private long _Id;
        public Session(long id)
        {
            _Id = id;
        }
        public long ID
        {
            get { return _Id; }
            set { _Id = value; }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Session))
            {
                return false;
            }
            Session s = (Session)obj;
            return _Id == s.ID;
        }
        public static bool operator ==(Session s1, Session s2)
        {
            return s1.ID == s2.ID;
        }

        public static bool operator !=(Session s1, Session s2)
        {
            return s1.ID != s2.ID;
        }

        public override int GetHashCode()
        {
            return _Id.GetHashCode();
        }

        public override string ToString()
        {
            return _Id.ToString();
        }

        public static bool TryParse(string sessionstr,out Session session )
        {
            long id;
            bool result;
            if (long.TryParse(sessionstr, out id))
            {
                session = new Session(id);
                result = true;
            }
            else
            {
                session = InvalidValue;
                result = false;
            }
            return result;
        }
    }
}
