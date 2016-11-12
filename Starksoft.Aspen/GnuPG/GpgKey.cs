//
//  Author:
//       Benton Stark <benton.stark@gmail.com>
//
//  Copyright (c) 2016 Benton Stark
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Starksoft.Aspen.GnuPG
{
    /// <summary>
    /// Class structure that proves a read-only view of the GnuPG keys. 
    /// </summary>
    public class GpgKey
    {
        private string _key;
        private DateTime _keyCreation;
	private DateTime _keyExpiration;
        private string _userId;
        private string _userName;
        private string _subKey;
        private DateTime _subKeyExpiration;
        private string _raw;

        /// <summary>
        /// GnuPGKey constructor.
        /// </summary>
        /// <param name="raw">Raw output stream text data containing key information.</param>
        public GpgKey(string raw)
        {
            _raw = raw;
            ParseRaw();          
        }

        /// <summary>
        /// Key text information.
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Key creation date and time (if available otherwise DateTime.MinValue).
        /// </summary>
        public DateTime KeyCreation
        {
            get { return _keyCreation; }
        }

        /// <summary>
        /// Key expiration date and time.
        /// </summary>
        public DateTime KeyExpiration
        {
            get { return _keyExpiration; }
        }

        /// <summary>
        /// Key user identification.
        /// </summary>
        public string UserId
        {
            get { return _userId; }
        }

        /// <summary>
        /// Key user name.
        /// </summary>
        public string UserName
        {
            get { return _userName; }
        }

        /// <summary>
        /// Sub-key information.
        /// </summary>
        public string SubKey
        {
            get { return _subKey; }
        }

        /// <summary>
        /// Sub-key expiration data and time.
        /// </summary>
        public DateTime SubKeyExpiration
        {
            get { return _subKeyExpiration; }
        }

        /// <summary>
        /// Raw output key text generated by GPG.EXE.
        /// </summary>
        public string Raw
        {
            get { return _raw; }
        }

        /// <summary>
        /// Parse the raw console data as provided by gpg output.
	/// </summary>
        private void ParseRaw()
        {
            // split the lines either CR or LF and then remove the empty entries
            // this will allow the solution to work both Linux and Windows 
            string rawClean = _raw;
            rawClean = rawClean.Replace("[", "");
            rawClean = rawClean.Replace("]", "");
            string[] lines = rawClean.Split(  new char[] { '\r', '\n' }, 
                                StringSplitOptions.RemoveEmptyEntries);

            string[] pub = SplitSpaces(lines[0]);
            string uid = null;
            if (lines.Length > 1)
                uid = lines[1];
            string[] sub = null;
            if (lines.Length < 1)
                sub = SplitSpaces(lines[2]);
                        
            _key = pub[1];
            if (pub.Length >= 6) {	
                if (pub[5].Length > 1)
                    _keyExpiration = DateTime.Parse(pub[5].Substring(0, pub[5].Length-1));
                    _keyCreation = DateTime.Parse(pub[2]);
                } 
                else 
                {
                    // try to parse it
                    DateTime.TryParse(pub[2], out _keyExpiration);
                }
                // test to see if there is a sub key
                if (sub != null) {
                    _subKey = sub[1];
                DateTime.TryParse(sub[2], out _subKeyExpiration);
            }

            // test to see if we have a uid and if so try to parse it
            if (uid != null)
                ParseUid(uid);
        }

        private string[] SplitSpaces(string input)
        {
            char[] splitChar = { ' ' };
            return input.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
        }
             
        private void ParseUid(string uid)
        {
            Regex name = new Regex(@"(?<=uid).*(?=<)");
            Regex userId = new Regex(@"(?<=<).*(?=>)");

            _userName = name.Match(uid).ToString().Trim();
            _userId = userId.Match(uid).ToString();
        }

        /// <summary>
        /// Returns string reprentation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat("Key: {0}{1}", _key, Environment.NewLine);
            s.AppendFormat("KeyCreation: {0}{1}", _keyCreation, Environment.NewLine);
            s.AppendFormat("KeyExpiration: {0}{1}", _keyExpiration, Environment.NewLine);
            s.AppendFormat("UserId: {0}{1}", _userId, Environment.NewLine);
            s.AppendFormat("SubKey: {0}{1}", _subKey, Environment.NewLine);
            s.AppendFormat("SubKeyExpiration: {0}{1}", _subKeyExpiration, Environment.NewLine);
            s.AppendFormat("Raw: {0}{1}", _raw, Environment.NewLine);
            return s.ToString();
        }

    }
}
