# GoFind README

GoFind [[1]](#gofind) uses content-based multimedia retrieval engine [Cineast](https://github.com/vitrivr/cineast/)
to bring spatio-temporal image retrieval to mobile devices.
Ultimately, _GoFind_ is a mobile browser for historic images in their real-world context.
The combination of content-based image retrieval in spatio-temporal domain with
the mobile device's camera-feed creates an immersive display for historic images
in their real-world context.
There are two display modes: Blend Display and AR Display. The former enables users to superimpose
the retrieved image with the device's camera feed and the latter positions the retrieved image
at it's real-world location as an augmented reality object.

## Demo

See the [demo](https://youtu.be/W14e8SRZGkA) on Youtube

## System

The system is comprised of two components: The back-end which is a Cineast instance
and the front-end, which is built using [Unity3d](https://unity3d.com) and [AR Core](https://developers.google.com/ar/).
The [UnityInterface](https://github.com/vitrivr/UnityInterface) of vitrivr is used to connect to the backend.


<a name="gofind">[1]</a>: L. Sauter, L. Rossetto and H. Schuldt
Exploring Cultural Heritage in Augmented Reality with GoFind!
2018 IEEE International Conference on Artificial Intelligence and Virtual Reality (AIVR)
Taichung, Taiwan, 2018, pp. 187-188. https://doi.org/10.1109/AIVR.2018.00041
