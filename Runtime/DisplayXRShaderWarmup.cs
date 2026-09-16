// Copyright 2024-2026, DisplayXR contributors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace DisplayXR
{
    /// <summary>
    /// One-shot D3D12/Vulkan PSO warmup. First-use shader compiles showed up in
    /// the profiler as 200–360 ms main-thread stalls with an idle GPU, then a
    /// fat present on the next frame. Pulling that work into splash / session
    /// start keeps it off the gameplay thread.
    /// </summary>
    internal static class DisplayXRShaderWarmup
    {
        static bool s_done;

        internal static void Run()
        {
            if (s_done) return;
            s_done = true;
            Shader.WarmupAllShaders();
        }

        /// <summary>
        /// Second pass after the real scene has drawn into the XR eye targets.
        /// Picks up URP stereo variants WarmupAllShaders missed while the
        /// camera was on the splash-only mask.
        /// </summary>
        internal static void RunAgain()
        {
            s_done = false;
            Run();
        }
    }
}
