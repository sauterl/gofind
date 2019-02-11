using System.Collections.Generic;

namespace Assets.Scripts.IO
{
    [System.Serializable]
    public class HeadingDictionary
    {
        [System.Serializable]
        public class HeadingEntry
        {
            public string id;
            public float heading;

            public HeadingEntry() { }

            public HeadingEntry(string id, float heading)
            {
                this.id = id;
                this.heading = heading;
            }

            public override bool Equals(object obj)
            {
                if (obj.GetType() == typeof(HeadingEntry))
                {
                    HeadingEntry he = (HeadingEntry) obj;
                    return id.Equals(he.id);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return id.GetHashCode() + heading.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("HeadingEntry [id={0}, heading={1}]", id, heading);
            }
        }

        public List<HeadingEntry> entries;

        public HeadingDictionary()
        {
            entries = new List<HeadingEntry>();

        }

        public int Count() {
            return entries.Count;
        }

        public bool IsEmpty() {
            return Count() == 0;
        }

        public void Add(string id, float heading)
        {
            entries.Add(new HeadingEntry(id, heading));
        }

        public float GetHeading(string id)
        {
            foreach (HeadingEntry entry in entries)
            {
                if (entry.id.Equals(id))
                {
                    return entry.heading;
                }
            }
            return float.NaN;
        }
    }
}