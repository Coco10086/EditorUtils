using UnityEngine;  
using System;  
public class ProfilerSample {  
    public static bool EnableProfilerSample = true;  
    public static bool EnableFormatStringOutput = true;// �Ƿ�����BeginSample�Ĵ��������ʹ�ø�ʽ���ַ�������ʽ���ַ������������ڴ濪����  
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
           // ��Ҫʱ�����ã���string.Format��������GC Alloc����Ҫ����  
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