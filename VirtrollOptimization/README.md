# Scalarm Virtroll Optimization Supervisor

This is a Scalarm Virtroll Optimization Supervisor for standalone use with Scalarm Platform or as a Scalarm Experiment Supervisor plugin application. For more information about Scalarm Supervisor see: https://github.com/Scalarm/scalarm_experiment_supervisor

# Dependencies

This program requires following libraries placed in its root directory to run:
* ``Newtonsoft.Json.dll`` - version >= 6.0.8
* ``RestSharp.dll`` - version >= 105.0.1
* ``Scalarm.dll`` - can be found here: https://github.com/Scalarm/scalarm_client_csharp
* ``Optimization.dll`` - a proprietary optimization library written for Virtroll project; not available publicly at the moment

# Building

Program builds in .Net4.0 compiler - both Mono and VisualStudio, but only Mono is officially supported and tested. The easiest way is to open project file in IDE, satisfy dependencies (listed above) and build the project.

# Execution

Please put all dependencies DLLs in compiled program root directory and launch VirtrollOptimizationMain.exe with Mono/.Net runtime.
**At least Mono 4.2 runtime is required to run.** The program will not run under Mono 3.x!


## Execution parameters

* ``mono VirtrollOptimizationMain.exe`` will read config from ``config.json`` in current directory
* ``mono VirtrollOptimizationMain.exe -config <config_path>`` will read config from ``<config_path>`` file
* ``mono VirtrollOptimizationMain.exe -stdin`` will read config from standard input (e.g. ``cat config.json | Program.exe`` on \*NIX)


# Configuration

## Example configurations

### Genetic algorithm

```json
{
  "method_type": "genetic",
  "experiment_id": "56b1b4f9f83f4910c0000261",
  "user": "f784b56e-e02c-4ede-9d85-fb3ed5566cc5",
  "password":" <ciach>",
  "parameters": [
    {
      "id":"parameter1",
      "min":0,
      "max":1000
    },
    {
      "id":"parameter2",
      "min":-100,
      "max":100
    }
  ],
  "address": "system.scalarm.com"
}
```

## Configuration keys

* ``method_type``: string, name of SA method to use:
  * ``genetic``, when using, set additional keys in config:
    * TODO
  * ``hooke_jeeves``, when using, set additional keys in config:
    * TODO
* ``address``: string, address of Scalarm Experiment Manager (without protocol provided)
* credentials - either BasicAuth login/password or X509 proxy certificate used to authenticate in Scalarm
  * BasicAuth
    * ``user``: string, username
    * ``password``: string, password
  * X509 Proxy certificate
    * ``experiment_manager_proxy_path``: string, path to file in PEM format X509 Proxy certificate
* entity to supervise - can be either Scalarm SimulationScenario or Experiment; of course these entities must exists and credentials should allow to view or/and modify it in provided Scalarm instance
  * SimulationScenario - if provided, ID of SimulationScenario will be used to instantiate an Experiment to supervise with sensitivity analysis algorithm
    * ``simulation_id``: string, ID of Scalarm SimulationScenario
  * Experiment - used only if SimulationScenario is not provided (used by Pathfinder)
    * ``experiment_id``: string, ID of Scalarm Experiment to use
* ``parameters``: array, an array of SimulationScenario parameters specifications
  * parameter object
    * ``id``: string
    * ~~``type``: string, currently not used - *all parameters should be float*~~
    * ``min``: float, lower limit of parameter value range to search
    * ``max``: float, upper limit of parameter value range to search
~~* ``fake_experiment`` (optional): boolean, if true - do not use real Scalarm server, randomly generate results instead and write optimization results to stdout~~

# Output

Depending on the configuration, results will be both send to Scalarm and printed on stdout, or only printed on stout (when ``fake_experiment: true``).

## Example outputs

### Genetic algorithm

TODO
