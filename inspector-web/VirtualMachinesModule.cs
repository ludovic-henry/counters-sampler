﻿using System;
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
		public VirtualMachinesModule ()
		{
			Get ["/virtual-machines"] = parameters => {
				var result = new List<object> ();

				foreach (var vm in VirtualMachine.GetVirtualMachines()) {
					try {
						var process = Process.GetProcessById ((int)vm.Pid);

						result.Add (new {
							pid = vm.Pid,
							name = process.ProcessName,
							is_server = vm.IsCurrent,
							is_current = (VirtualMachineModel.Current != null && VirtualMachineModel.Current.Pid == vm.Pid),
						});
					} catch (Exception e) {
						Debug.WriteLine ("Failed to read info on {0} due to {1}", vm.Pid, e.Message);
					}
				}

				return Response.AsJson (result);
			};

			Post ["/virtual-machines/attach/{pid}"] = parameters => {
				if (VirtualMachineModel.Current != null)
					InspectorModel.Inspector.Close ();

				int pid;

				if (!int.TryParse (parameters.pid, out pid))
					return Response.AsJson (new { status = "Bad pid : not an int. pid = " + parameters.pid.ToString ()}, HttpStatusCode.BadRequest);

				if (HistoryModel.History != null)
					HistoryModel.History.Clear ();

				VirtualMachineModel.Current = new VirtualMachine (pid);
				VirtualMachineModel.Current.StartPerfAgent ("interval=1000,address=file:///tmp/out.sample," +
					"counters=Mono GC/Created object count;Mono JIT/Compiled methods;Mono JIT/JIT trampolines");

				return Response.AsJson ("");
			};
		}
	}
}

