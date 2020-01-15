using UnityEngine;  
using System;
using UnityEngine.Profiling;

public class ProfilerSample {  
    public static bool EnableProfilerSample = true;  
    public static bool EnableFormatStringOutput = true;  
    public static void BeginSample(string name) {  
#if ENABLE_PROFILER  
        if(EnableProfilerSample){  
           Profiler.BeginSample(name);  
        }  
#endif  
    }  
    public static void BeginSample(string formatName, params object[] args) {  
#if ENABLE_PROFILER  
        if(EnableProfilerSample) {  
           if (EnableFormatStringOutput)  
               Profiler.BeginSample(string.Format(formatName, args));  
           else  
               Profiler.BeginSample(formatName);  
        }  
#endif  
    }  
    public static void EndSample() {  
#if ENABLE_PROFILER  
        if(EnableProfilerSample) {  
           Profiler.EndSample();  
        }  
#endif  
    }  
}  