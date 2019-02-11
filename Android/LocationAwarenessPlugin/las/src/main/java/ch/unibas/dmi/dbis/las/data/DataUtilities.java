package ch.unibas.dmi.dbis.las.data;

import android.location.Location;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.IOException;

/**
 * Created by Loris on 19.06.2017.
 */

public class DataUtilities {

    private static final ObjectMapper MAPPER = new ObjectMapper();

    private DataUtilities(){
        // no instance needed
    }

    public static LocationObject readLocationObject(String json) throws IOException {
        return MAPPER.readValue(json, LocationObject.class);
    }

    public static Configuration readConfig(String json) throws IOException{
        return MAPPER.readValue(json, Configuration.class);
    }

    public static LocationTargetList readLocationTargetList(String json) throws IOException{
        return MAPPER.readValue(json, LocationTargetList.class);
    }

    public static float distanceBetween(Location loc, LocationObject obj){
        Location locObj = convertFromObject(obj);

        return loc.distanceTo(locObj); // in meters
    }

    static Location convertFromObject(LocationObject obj){
        Location locObj = new Location("json"); // Provider is needed
        locObj.setLatitude(obj.getLatitude());
        locObj.setLongitude(obj.getLongitude());
        return locObj;
    }

    public static String toJson(LocationObject obj) throws JsonProcessingException {
        return MAPPER.writeValueAsString(obj);
    }
}
