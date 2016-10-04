using System;
using Newtonsoft.Json.Linq;
using Scalarm;
using System.IO;
using System.Collections.Generic;
using Optimization.Core;
using System.Linq;
using Common;

namespace VirtrollOptimization
{
	/// <summary>
	/// Structurized configuration passed to Scalarm Supervisor
	/// See README.md in VirtrollOptimization project for configuration examples.
	/// </summary>
	public class SupervisorConfig
	{
		public string ExperimentManagerAddress { get; set; }
		public string ExperimentManagerUrl {
			get {
				return String.Format("https://{0}", this.ExperimentManagerAddress);
			}
		}

		public ScalarmParameter[] Parameters { get; set; }
		public string SimulationId { get; set; }
		public string ExperimentId { get; set; }

		public string MethodType { get; set; }
		public Boolean IsFakeExperiment { get; set; }

		public string ExperimentManagerProxyPath { get; set; }
		public string ExperimentManagerLogin { get; set; }
		public string ExperimentManagerPassword { get; set; }

		public JObject RawAppConfig { get; set; }

		/// <summary>
		/// Please do not use this constructor - use new(string) instead
		/// </summary>
		public SupervisorConfig() : base()
		{}

		public SupervisorConfig(string configText)
		{
			JObject appConfig = JObject.Parse(configText);
			this.RawAppConfig = appConfig;

			this.ExperimentManagerAddress = appConfig["address"].ToObject<string>();

			this.Parameters = appConfig["parameters"].ToObject<ScalarmParameter[]>();

			// TODO: remove code
//			var simulationIdJson = appConfig["simulation_id"];
//			this.SimulationId = (simulationIdJson != null) ? simulationIdJson.ToObject<string>() : null;

			this.SimulationId = OptionalJsonGet<string>(appConfig, "simulation_id", null);
			this.ExperimentId = OptionalJsonGet<string>(appConfig, "experiment_id", null);

			this.MethodType = appConfig["method_type"].ToObject<string>();

			this.IsFakeExperiment = OptionalJsonGet<Boolean>(appConfig, "fake_experiment", false);

			// If one of these is not speficied - will throw an error
			if (appConfig["experiment_manager_proxy_path"] != null) {
				this.ExperimentManagerProxyPath = appConfig["experiment_manager_proxy_path"].ToObject<string>();
			} else {
				this.ExperimentManagerLogin = appConfig["user"].ToObject<string>();
				this.ExperimentManagerPassword = appConfig["password"].ToObject<string>();
			}
		}

		/// <summary>
		/// Try to get a property value of T type from jObject JSON object.
		/// If property does not exists, return defaultValue (null, if not specified).
		/// </summary>
		public static T OptionalJsonGet<T>(JObject jObject, string property, T defaultValue) {
			var buffer = jObject[property];
			return buffer != null ? buffer.ToObject<T>() : defaultValue;
		}

		/// <summary>
		/// Instantiate Scalarm Client using proxy (if proxy path provided) or basic auth (if login/password provided)
		/// If IsFakeExperiment config flag is true, returns an instance of FakeSupervisedExperiment (see ScalarmClient for further information).
		/// Otherwise:
		/// If ExperimentId has been provided in config, returns an object of this experiment with client injected.
		/// If SimulationId has been provided in config, creates a new experiment using provided simulation scenario.
		/// </summary>
		public SupervisedExperiment GetExperiment() {
			SupervisedExperiment experiment = null;
			if (this.IsFakeExperiment) {
				experiment = new FakeSupervisedExperiment();
			} else {
				// create Scalarm Client basing on credentials from script config
				Scalarm.Client client = null;
				if (this.ExperimentManagerProxyPath != null) {
					client = new Scalarm.ProxyCertClient(this.ExperimentManagerUrl, new FileStream(this.ExperimentManagerProxyPath, FileMode.Open));
				} else {
					client = new Scalarm.BasicAuthClient(this.ExperimentManagerUrl, this.ExperimentManagerLogin, this.ExperimentManagerPassword);
				}
				// ---
				// use experiment or create new with simulation_id
				if (this.SimulationId != null && this.ExperimentId == null) {
					Logger.Info(string.Format("Using simulation {0}/simulations/{1} to instantiate experiment",  this.ExperimentManagerUrl, this.SimulationId));
					var scenario = client.GetScenarioById(this.SimulationId);
					experiment = scenario.CreateSupervisedExperiment(null, new Dictionary<string, object> {
						{"name", String.Format("Optimization of {0} using {0}", scenario.Name, this.MethodType)}
					});
					this.ExperimentId = experiment.Id;
				} else {
					Logger.Info(string.Format("Using experiment provided by ID: {0}", this.ExperimentId));
					experiment =
						client.GetExperimentById<Scalarm.SupervisedExperiment>(this.ExperimentId);
				}
			}

			Logger.Info(string.Format("Using experiment {0}/experiments/{1}", this.ExperimentManagerUrl, experiment.Id));
			return experiment;
		}

		/// <summary>
		/// Using Scalarm Input parameters definition from config, returns a List of Optimization.Core.InputProperties.
		/// </summary>
		/// <returns>The input properties.</returns>
		public List<InputProperties> GetInputProperties() {
			// assuming that key order is the same as in config file
			List<InputProperties> properties = new List<InputProperties>(this.Parameters.Count());
			{
				for (int i=0; i < this.Parameters.Count(); ++i) {
//					Logger.Info(this.Parameters[i].ToString());
					properties.Add(new InputProperties(i, this.Parameters[i].id, InputValuesType.Range, this.Parameters[i].Range));
				}
			}

			return properties;
		}
	}
}

 