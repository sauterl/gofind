using UnityEngine;

namespace Assets.GoFindMap.Scripts
{
    public class GeoPosition : MonoBehaviour
    {

        public GeoPosition()
        {
        }

        public GeoPosition(GeoLocation loc)
        {
            Location = loc;
        }

        public GeoLocation Location { get; set; }

    }
}
