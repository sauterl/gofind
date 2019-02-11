package ch.unibas.dmi.dbis.las;

import android.Manifest;
import android.app.Fragment;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import ch.unibas.dmi.dbis.las.annotation.UnityCallable;

/**
 * The main entry class.
 *
 * Will be called from Unity.
 *
 * Created by Loris on 19.06.2017.
 */

public class LocationAwarenessPlugin extends Fragment {

    public static final String PLUGIN_TAG = "LocationAwarenessPlugin";

    @UnityCallable
    public static LocationAwarenessPlugin instance = null;// public so that it could be called from unity

    // TODO tidy up and make instance stuff oop

    @Override
    public void onCreate(Bundle savedInstance){
        super.onCreate(savedInstance);
        setRetainInstance(true);
    }

    public static void init(){
        if(instance == null){
            instance = new LocationAwarenessPlugin();
            hookIntoUnity();
        }
        Log.i(PLUGIN_TAG, "Finished plugin initialization");
    }

    /**
     * Adds the instance of the plugin (the instance of this class, since its a Fragment) to the unity player
     */
    protected static void hookIntoUnity() { // BP
        if (UnityPlayer.currentActivity.getFragmentManager().findFragmentByTag(PLUGIN_TAG) == null) {
            UnityPlayer.currentActivity.getFragmentManager().beginTransaction().add(instance, PLUGIN_TAG).commit();
            Log.d(PLUGIN_TAG, "Added plugin to UnityPlayer");
        } else {
            Log.d(PLUGIN_TAG, "Plugin was added previously");
        }
    }

    @UnityCallable
    public void startService(String config, String targets){
        Log.d(PLUGIN_TAG, "Starting service...");
        Intent intent = new Intent(getActivity(), LocationAwareService.class);
        intent.putExtra(LocationAwareService.CONFIG_KEY, config);
        intent.putExtra(LocationAwareService.TARGET_LIST_KEY, targets);
        intent.putExtra(LocationAwareService.CALLER_CLASS_KEY, getActivity().getClass().getName());
        getActivity().startService(intent);
        Log.d(PLUGIN_TAG, "Started service");
    }

    @UnityCallable
    public void stopService(){
        Log.d(PLUGIN_TAG, "Stopping service...");
        getActivity().stopService(new Intent(getActivity(), LocationAwareService.class));
    }
}
