package ch.unibas.dmi.dbis.las.data;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

/**
 * Created by Loris on 19.06.2017.
 */

@JsonIgnoreProperties(ignoreUnknown =  true) // No confusion by ObjectMapper if unknown fields are specified
public class Configuration {

    private float minDistance; //m
    private float closeDistance;//m
    private long minTime;//ms

    public Configuration() {
    }

    public Configuration(float minDistance, float closeDistance, long minTime) {
        this.minDistance = minDistance;
        this.closeDistance = closeDistance;
        this.minTime = minTime;
    }

    public float getMinDistance() {
        return minDistance;
    }

    public void setMinDistance(float minDistance) {
        this.minDistance = minDistance;
    }

    public float getCloseDistance() {
        return closeDistance;
    }

    public void setCloseDistance(float closeDistance) {
        this.closeDistance = closeDistance;
    }

    public long getMinTime() {
        return minTime;
    }

    public void setMinTime(long minTime) {
        this.minTime = minTime;
    }
}
