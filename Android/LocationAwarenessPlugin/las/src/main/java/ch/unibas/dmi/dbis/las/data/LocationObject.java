package ch.unibas.dmi.dbis.las.data;

/**
 * Created by Loris on 19.06.2017.
 */

public class LocationObject {

    private String id;
    private double latitude;
    private double longitude;

    public LocationObject() {
    }

    public LocationObject(String id, double latitude, double longitude) {
        this.id = id;
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public String getId() {
        return id;
    }

    public void setId(String id) {
        this.id = id;
    }

    public double getLatitude() {
        return latitude;
    }

    public void setLatitude(double latitude) {
        this.latitude = latitude;
    }

    public double getLongitude() {
        return longitude;
    }

    public void setLongitude(double longitude) {
        this.longitude = longitude;
    }
}
