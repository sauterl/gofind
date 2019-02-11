package ch.unibas.dmi.dbis.las;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.app.TaskStackBuilder;
import android.content.Context;
import android.content.Intent;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.os.IBinder;
import android.support.annotation.Nullable;
import android.util.Log;

import java.io.IOException;

import ch.unibas.dmi.dbis.las.data.Configuration;
import ch.unibas.dmi.dbis.las.data.DataUtilities;
import ch.unibas.dmi.dbis.las.data.LocationObject;
import ch.unibas.dmi.dbis.las.data.LocationTargetList;

import static ch.unibas.dmi.dbis.las.LocationAwarenessPlugin.PLUGIN_TAG;

/**
 * Created by Loris on 19.06.2017.
 */

public class LocationAwareService extends Service implements LocationListener {


    public static final String TARGET_LIST_KEY = "targets";
    public static final String CALLER_CLASS_KEY = "callerClass";
    public static final String CONFIG_KEY = "config";

    public static final Configuration DEFAULT_CONFIG = new Configuration(2f, 10f, 30 * 1000);

    private static final String TAG = "LocationAwareService";
    private boolean gpsProviderAvailable = false;
    private boolean networkProviderEnabled = false;
    private LocationManager locationManager;
    private Configuration configuration;
    private LocationTargetList targetList = null;
    private Class<?> callerClass = null;
    private Location location;

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null; // Purposely, since service should not be bound
    }

    @Override
    public void onCreate() {
        Log.d(TAG, "onCreate");
        aquireLocationManager();
        checkProviderAvailability();


    }

    @Override
    public void onDestroy() {
        Log.d(TAG, "onDestroy");
        super.onDestroy();
        stopLocationUpdates();
    }

    private void aquireLocationManager() {
        Log.d(TAG, "aquireLocationManager");
        locationManager = (LocationManager) getApplicationContext().getSystemService(LOCATION_SERVICE);
    }

    private void checkProviderAvailability() {

        gpsProviderAvailable = locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER);

        networkProviderEnabled = locationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER);
    }

    private void requestNetworkUpdates() {
        if (networkProviderEnabled) {
            try {
                locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, configuration.getMinTime(), configuration.getMinDistance(), this);
                Log.d(TAG, "Requested network updates");
            } catch (SecurityException ex) {
                Log.e(TAG, "No access to LocationManager.NETWORK_PROVIDER", ex);
            }

        } // ENDIF NETWORK
    }

    private void requestGpsUpdates() {
        if (gpsProviderAvailable) {
            try {
                locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, configuration.getMinTime(), configuration.getMinDistance(), this);
                Log.d(TAG, "Requested gps updates");
            } catch (SecurityException ex) {
                Log.e(TAG, "No access to Location.GPS_PROVIDER", ex);
            }

        }
    }

    private void updateLocationData(Location location) {
        if (location != null) {
            Log.d(TAG, "Received location: " + location.toString());

            this.location = location;

        }
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        Log.d(TAG, "onStartCommand");
        super.onStartCommand(intent, flags, startId);
        Log.d(TAG, "Intent: " + intent.toString());
        Log.d(TAG, "Targets: " + intent.getStringExtra(TARGET_LIST_KEY));
        try {
            targetList = DataUtilities.readLocationTargetList(intent.getStringExtra(TARGET_LIST_KEY));
            Log.i(TAG, "Read target list");
        } catch (IOException e) {
            Log.e(TAG, "IOException during read of target list.", e);
            // TODO Die
        }
        try {
            configuration = DataUtilities.readConfig(intent.getStringExtra(CONFIG_KEY));
            Log.i(TAG, "Parsed configuration");
        } catch (IOException e) {
            Log.e(TAG, "IOException during parsing of servicesetup", e);
            configuration = DEFAULT_CONFIG;
        }
        String callerClassName = intent.getStringExtra(CALLER_CLASS_KEY);
        try {
            callerClass = Class.forName(callerClassName);
        } catch (ClassNotFoundException e) {
            Log.e(TAG, "Could not find caller class", e);
            // TODO die
        }
        Log.d(TAG, "Starting location updates");
        requestGpsUpdates();
        requestNetworkUpdates();
        return START_REDELIVER_INTENT; // The intent, more precisely the intent's extra is important
    }

    @Override
    public void onLocationChanged(Location location) {
        // Handle updates here
        Log.d(TAG, "onLocationChanged: " + location.toString());
        updateLocationData(location);
        // TODO Fancy update and location interpolation stuff and things

        // Check if last known location is in close range to a target
        if (targetList != null) {
            if ((targetList.getTargets() != null) && (targetList.getTargets().length > 0)) {
                for (LocationObject obj : targetList.getTargets()) {
                    if (isInCoseRange(location, obj, configuration.getCloseDistance())) { // 5m -> via ServiceSetup from unity defined
                        //startCallingActivityAndDie();

                        // TODO: Replace startCallingActivityAndDie with sendNotification

                        notifyUser(DataUtilities.distanceBetween(location, obj));
                    }
                }
            }
        }
    }

    private void startCallingActivityAndDie() {
        Log.d(TAG, "startCallingActivityAndDie");
        if (callerClass != null) {
            Intent intent = new Intent(this, callerClass);
            intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
            startActivity(intent);
            stopSelf();
        }
    }

    private void notifyUser(double distance){
        Log.d(TAG, "notifyUser");

        if(callerClass != null){
            buildAndShow("Target reached!", "Distance to target: "+distance, 1);
        }

    }

    public void buildAndShow(String title, String text, int ID){
        Log.i(TAG, "Creating a notification");
        Context cxt = getApplicationContext();

        Notification.Builder builder = new Notification.Builder(cxt );
        builder.setSmallIcon(android.R.drawable.btn_star_big_on);
        builder.setContentText(text);
        builder.setContentTitle(title);

        // Create an intent which will start unity app (since fragment is added to unity app)
        Intent resultIntent = new Intent(cxt, callerClass);

        TaskStackBuilder stackBuilder = TaskStackBuilder.create(cxt );
        stackBuilder.addParentStack(callerClass);
        stackBuilder.addNextIntent(resultIntent);
        PendingIntent resultPendingIntent= stackBuilder.getPendingIntent(0, PendingIntent.FLAG_UPDATE_CURRENT);

        builder.setContentIntent(resultPendingIntent);
        NotificationManager manager = (NotificationManager)getSystemService(Context.NOTIFICATION_SERVICE);
        builder.setShowWhen(true);
        builder.setWhen(System.currentTimeMillis());
        builder.setPriority(Notification.PRIORITY_MAX);
        builder.setDefaults(Notification.DEFAULT_VIBRATE);
        manager.notify(ID,builder.build());


        Log.i(TAG, "Notification was shipped");
    }

    @Override
    public void onStatusChanged(String provider, int status, Bundle extras) {
        // handle status changes
        Log.d(TAG, "onStatusChanged: " + provider);
    }

    @Override
    public void onProviderEnabled(String provider) {
        // Handle provider enabled here
        Log.d(TAG, "onProviderEnabled: " + provider);
    }

    @Override
    public void onProviderDisabled(String provider) {
        // handle provider disabled here
        Log.d(TAG, "onProvderDisabled: " + provider);
    }

    public void stopLocationUpdates() {
        if (locationManager != null) {
            locationManager.removeUpdates(this);
        }
    }

    private boolean isInCoseRange(Location location, LocationObject obj, float range) {
        return DataUtilities.distanceBetween(location, obj) <= range;
    }
}
