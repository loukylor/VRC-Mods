using System;
using System.Collections;
using System.Collections.Generic;

namespace AskToPortal
{
    public class RoomInfo
    {
        public string instanceId;
        public string instanceType;
        public string region;
        public string ownerId;
        public string nonce;

        public List<string> errors = new List<string>();

        public RoomInfo()
        {

        }

        // Example invite room id: instanceId~private(someones user id here)~nonce(Long hex code here)
        // Example invite+ room id: instanceId~private(someones user id here)~canRequestInvite~nonce(Long hex code here)
        // Example friends room id: instanceId~friend(someones user id here)~nonce(Long hex code here)
        // Example friends+ room id: instanceId~hidden(someones user id here)~nonce(Long hex code here)
        // Example public room id: instanceId

        // Example public us room id: instanceId
        // Example invite us room id: instanceId~private(someones user id here)~nonce(Long hex code here)
        // Example public eu room id: instanceId~region(eu)
        // Example invite jp room id: instanceId~private(someones user id here)~region(jp)~nonce(Long hex code here)
        public RoomInfo(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                errors.Add("Room ID was empty or null");
                return;
            }

            IEnumerator splitString = roomId.Split(new char[1] { '~' }).GetEnumerator();
            splitString.MoveNext();
            instanceId = (string)splitString.Current;
            try
            {
                int instanceId = int.Parse(this.instanceId);
                if (instanceId > 99998 || instanceId < 1) 
                    // If the instance ID is out of range of the regular ID
                    errors.Add("Instance ID was too large or small");
            }
            catch
            {
                // If int.Parse errored meaning the instance ID is not a regular number
                errors.Add("Instance ID was in an invalid format");
            }

            if (!splitString.MoveNext())
            {
                // If the ID ends there that means its a public US instance
                instanceType = "Public";
                region = "US";
                return;
            }

            Tag tempTag;
            try
            {
                tempTag = new Tag((string)splitString.Current);
                string tagType = ParseAmbiguousTag(tempTag);
                if (tagType == "nonce")
                    errors.Add("Nonce tag found before the instance type tag");
            }
            catch
            {
                errors.Add("Failed to parse instance type/region tag");
                return;
            }

            if (!splitString.MoveNext())
            {
                // If the ID ends here it means it's a public non US region instance
                instanceType = "Public";
                return;
            }

            if ((string)splitString.Current == "canRequestInvite")
            {
                if (instanceType == "Invite Only" && region == null)
                    instanceType = "Invite+";
                else if (region != null)
                    errors.Add("canRequestInvite tag was found before the instance type tag");
                else
                    errors.Add("canRequestInvite tag was found despite the instance not being an invite type");

                splitString.MoveNext();
            }

            try
            {
                tempTag = new Tag((string)splitString.Current);
                if (ParseAmbiguousTag(tempTag) == "nonce")
                {
                    if (region == null)
                        region = "US";
                    else
                        errors.Add("Nonce tag found before the instance type tag");
                    return;
                }
            }
            catch
            {
                errors.Add("Failed to parse instance type/region/nonce tag");
                return;
            }


            if (!splitString.MoveNext())
            {
                // The nonce tag is garunteed in non public instances
                errors.Add("Nonce tag did not exist despite the instance type being non-public");
                return;
            }

            if ((string)splitString.Current == "canRequestInvite")
            {
                if (instanceType == "Invite Only")
                {
                    instanceType = "Invite+";
                    splitString.MoveNext();
                }
                else
                {
                    errors.Add("canRequestInvite tag was found despite the instance not being an invite type");
                }
            }

            // This tag has to be the nonce tag
            try
            {
                Tag nonce = new Tag((string)splitString.Current);

                if (nonce.key != "nonce")
                {
                    errors.Add("A different tag was present where nonce was supposed to be");
                    return;
                }
                if (string.IsNullOrWhiteSpace(nonce.value))
                {
                    errors.Add("Nonce had an invalid value");
                    return;
                }
            }
            catch
            {
                errors.Add("Failed to parse nonce tag");
                return;
            }
        }

        private string ParseAmbiguousTag(Tag tag)
        {
            if (tag.key == "region")
            {
                // If it is the region tag
                switch (tag.value)
                {
                    case "eu":
                        region = "Europe";
                        break;
                    case "jp":
                        region = "Japan";
                        break;
                    default:
                        errors.Add("Region tag exists, but value was not recognized");
                        break;
                }
                return "region";
            }
            else if (tag.key == "nonce")
            {
                nonce = tag.value;
                if (string.IsNullOrWhiteSpace(tag.value))
                    errors.Add("Nonce had an invalid value");
                return "nonce";
            }
            else
            {
                // If it is not the region tag
                switch (tag.key)
                {
                    case "private":
                        instanceType = "Invite Only";
                        break;
                    case "friends":
                        instanceType = "Friends Only";
                        break;
                    case "hidden":
                        instanceType = "Friends+";
                        break;
                    default:
                        errors.Add("Instance type tag was not recognized.");
                        break;
                }
                ownerId = tag.value;
                if (string.IsNullOrWhiteSpace(ownerId))
                    errors.Add("Instance owner ID was invalid");
                return "instanceType";
            }
        }

        public struct Tag
        {
            public string key;
            public string value;

            public Tag(string tag)
            {
                try
                {
                    string[] tempString = tag.Split('(');

                    key = tempString[0];
                    value = tempString[1].TrimEnd(')');
                }
                catch
                {
                    throw new ArgumentException("Failed to parse tag");
                }
            }
            public Tag(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }
    }
}
