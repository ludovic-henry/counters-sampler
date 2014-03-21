using System;
using Nancy;
using Mono.Attach;
using System.Diagnostics;
using System.Collections.Generic;
using MonoCounters.Web.Models;
using System.Threading;

namespace MonoCounters.Web
{
    public class VirtualMachinesModule : NancyModule
    {
        public VirtualMachinesModule()
        {
            Get["/virtual-machines"] = parameters =>
            {
                var result = new List<object>();

                foreach (var vm in VirtualMachine.GetVirtualMachines())
                {
                    var process = Process.GetProcessById((int) vm.Pid);

                    result.Add(new {
                        pid = vm.Pid,
                        is_current = vm.IsCurrent,
                        name = process.ProcessName
                    });
                }

                return Response.AsJson(result);
            };

            Post["/virtual-machines/attach/{pid}"] = parameters =>
            {
                if (VirtualMachineModel.Current != null)
                {
                    Debug.WriteLine("Close connection to previous VirtualMachine", "VirtualMachinesModule.Attach");
                    InspectorModel.Inspector.Close();
                }

                int pid;

                if (!int.TryParse(parameters.pid, out pid))
                {
                    Debug.WriteLine("Bad pid : not an int. pid = " + parameters.pid.ToString(), "VirtualMachinesModule.Attach");
                    return Response.AsJson(new { status = "Bad pid : not an int. pid = " + parameters.pid.ToString()}, HttpStatusCode.BadRequest);
                }

                Debug.WriteLine("Trying to attach to pid " + pid.ToString(), "VirtualMachinesModule.Attach");

                VirtualMachineModel.Current = new VirtualMachine(pid);
                VirtualMachineModel.Current.StartPerfAgent("interval=1000,address=127.0.0.1:8888,counters=Mono GC/Created object count;Mono JIT/Compiled methods;Mono JIT/JIT trampolines");

                Debug.WriteLine("Attached to pid " + pid.ToString(), "VirtualMachinesModule.Attach");

                return Response.AsJson("");
            };
        }
    }
}

