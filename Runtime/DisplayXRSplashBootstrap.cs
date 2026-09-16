// Copyright 2024-2026, DisplayXR contributors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace DisplayXR
{
    /// <summary>
    /// Boot-splash spawner. The DisplayXR splash is <b>on by default</b>: every
    /// DisplayXR app shows it at startup with no scene wiring. Overlay mode
    /// hijacks the scene camera's culling mask (it does NOT spawn a second XR
    /// camera — destroying one mid-run recreates the eye RTs and hitchs). Then
    /// it restores the camera and destroys itself.
    ///
    /// Opt out by defining <c>DISPLAYXR_NO_SPLASH</c> (DisplayXR Manifest
    /// Settings ▸ Boot Splash ▸ Disable). When opted out the whole class
    /// compiles away and the companion build processor leaves Unity's stock
    /// splash alone.
    /// </summary>
#if !DISPLAYXR_NO_SPLASH
    internal static class DisplayXRSplashBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            // Runtime opt-out (debugging / editor): the compile-define opt-out above
            // can't be toggled without a recompile, so also honor an env var.
            if (System.Environment.GetEnvironmentVariable("DISPLAYXR_NO_SPLASH") != null)
                return;

            // Skip for transparent-overlay apps. They run in a cloaked, shifted-
            // offscreen overlay window (set up at SubsystemRegistration, before
            // this runs); the splash rig's window/viewport push pulls that window
            // back on-screen, and an opaque branded splash over a click-through
            // desktop overlay is wrong anyway. The flag is set by
            // DisplayXRTransparentOverlay.RequestTransparentSession() →
            // DisplayXRProvider.RequestTransparentBackground() at
            // SubsystemRegistration, which always precedes BeforeSceneLoad.
            if (DisplayXRProvider.s_TransparentBackgroundRequested)
                return;

            // Overlay splash must NOT be a second XR camera. Unity's XR display
            // provider renders every enabled stereo camera into the eye targets;
            // creating one here and Destroy()ing it a few seconds later is
            // XRTextureManager::RequestDestroyTexture — a multi-hundred-ms hitch
            // (and a 2+ s stall when it coincides with PSO warmup). Hijack the
            // scene camera's culling mask instead; no extra eye RT is allocated.
            var go = new GameObject("DisplayXR Boot Splash");
            Object.DontDestroyOnLoad(go);
            var splash = go.AddComponent<DisplayXRSplash>();
            splash.overlayMode = true;
            DisplayXRSplash.OverlayActive = true;
        }
    }
#endif
}
