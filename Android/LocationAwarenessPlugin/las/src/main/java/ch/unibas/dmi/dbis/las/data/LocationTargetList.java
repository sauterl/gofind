package ch.unibas.dmi.dbis.las.data;

/**
 * Created by Loris on 19.06.2017.
 */

public class LocationTargetList {

    private long timestamp;

    private LocationObject[] targets;

    public LocationTargetList() {
    }

    public LocationTargetList(long timestamp, LocationObject[] targets) {
        this.timestamp = timestamp;
        this.targets = targets;
    }

    public long getTimestamp() {
        return timestamp;
    }

    public void setTimestamp(long timestamp) {
        this.timestamp = timestamp;
    }

    public LocationObject[] getTargets() {
        return targets;
    }

    public void setTargets(LocationObject[] targets) {
        this.targets = targets;
    }
}
