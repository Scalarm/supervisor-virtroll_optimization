# Scalarm Virtroll Optimization Supervisor

This is a Scalarm Virtroll Optimization Supervisor for standalone use with Scalarm Platform or as a Scalarm Experiment Supervisor plugin binaries. For more information about Scalarm Supervisor see: https://github.com/Scalarm/scalarm_experiment_supervisor

# Introduction

The optimization supervisor is a Scalarm client (via RESTful interface) which controls execution of an experiment created from some simulation scenario.

## Scalarm simulation scenario

The simulation scenario to supervise must contain **only** float input parameters and at least one float output parameter, which will be used as a evaluation result.
It's id for simulation scenario should be set using ``moe_name`` parameter in configuration (see "Configuration" section in this readme).

## Supervisor

The supervisor application sets the input parameter space and waits for results (an "evaluation").
Note, that computational resources should be managed manually, and are not managed by supervisor application!
When available, fetched results are used by optimization algorithms (from Optimization library) and input parameter space can extended if needed (evaluation of new points).

When optimization is done by algorithm, the supervisor sends results of optimization to Scalarm, which can be accessed either by web GUI or REST API.
The format of results is described in "Output" section of this readme.

# Dependencies

This program requires following libraries to run:
* ``Newtonsoft.Json.dll`` - version >= 6.0.8 (available in NuGet)
* ``RestSharp.dll`` - version >= 105.0.1 (available in NuGet)
* ``Scalarm.dll`` - can be found here: https://github.com/Scalarm/scalarm_client_csharp
* ``Optimization.dll`` - a proprietary optimization library written for Virtroll project; not available publicly at the moment

# Building

Program builds in .Net4.0 compiler - both Mono and VisualStudio, but only Mono is officially supported and tested. The easiest way is to open project file in IDE, satisfy dependencies (listed above) and build the project.

You should put libraries listed in **Dependencies** section in ``libs/`` directory in solution root. Alternatively, these dependencies can be added manually in IDE (e.g. as references to VS projects).

If you have ``xbuild`` installed (e.g. with Mono on Linux) you can simply execute ``build.sh`` script in solution root - it will build project in Release mode in ``VirtrollOptimization/bin/Release/``.

# Execution

Please put all dependency DLLs in compiled program directory and launch VirtrollOptimizationMain.exe with Mono/.Net runtime.

**At least Mono 4.2 runtime is required to run.** The program will not run under Mono 3.x!


## Execution parameters

Below examples are for executing with Mono runtime. If executing in Windows (.Net), simply omit ``mono`` command.

* ``mono VirtrollOptimizationMain.exe`` will read config from ``config.json`` in current directory
* ``mono VirtrollOptimizationMain.exe -config <config_path>`` will read config from ``<config_path>`` file
* ``mono VirtrollOptimizationMain.exe -stdin`` will read config from standard input (e.g. ``cat config.json | Program.exe`` on \*NIX)


# Configuration

A configuration for supervisor appliaction is in JSON format.

## Configuration keys

Configuration keys can be divided into few groups described below.

### Common Scalarm configuration keys

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
* ``fake_experiment`` (optional): boolean, if true - do not use real Scalarm server, randomly generate results instead and write optimization results to stdout

### Common optimization configuration keys

* ``method_type``: string, name of optimization method to use:
  * ``genetic``
  * ``hooke_jeeves``
  * ``pso``

After setting the method type, use following configuration keys for configuring the selected algorithm.
NOTE: see ``VirtrollOptimization/OptimizationSupervisorConfig`` for default properties values.

#### Genetic algorithm configuration

* ``genetic_population_start`` (integer > 0)
* ``genetic_population_max`` (integer > 0)
* ``genetic_mutations_count`` (integer > 0)
* ``genetic_crosses_count`` (integer > 0)

#### Hooke-Jeeves algorithm configuration (CURRENTLY DISABLED!)

* ``hj_working_step_multiplier`` (integer > 0)
* ``hj_parallel`` (boolean)
* ``hj_step_sizes`` (array of floats) - the array should have as many elements as number of parameters used in scenario simulation.
  You should know an order of parameters defined for scenario simulation. Each element in the array is a step size for a parameter.

#### PSO algorithm configuration

* ``pso_particles_count`` (integer > 0)


## Example configurations

Examples of full configurations. The example configurations passed e.g. as a ``config.json`` file (see "Execution parameters" readme section for details about passing configuration).

Example execution:

```
mono VirtrollOptimizationMain.exe -config config.json
```

If example configuration is saved as ``config.json`` and is placed in working dir.

See also ``VirtrollOptimization/config.json.example`` for example config (some of keys are "commented out" in that file).

### Genetic algorithm

```json
{
  "address": "system.scalarm.com",
  "experiment_id": "56b1b4f9f83f4910c0000261",
  "user": "f784b56e-e02c-4ede-9d85-fb3ed5566cc5",
  "password":" <some-password>",
  "parameters": [
    {
      "id":"parameter1",
      "type": "float",
      "min":0,
      "max":1000
    },
    {
      "id":"parameter2",
      "type": "float",
      "min":-100,
      "max":100
    }
  ],

  "moe_name": "z",

  "optimization_max_error": 0.0000001,
  "optimization_max_iterations": 16,

  "method_type": "genetic",

  "genetic_population_start": 12,
  "genetic_population_max": 20,
  "genetic_mutations_count": 3,
  "genetic_crosses_count": 3
}
```

### Hooke-Jeeves algorithm

```json
{
  "address": "system.scalarm.com",
  "experiment_id": "56b1b4f9f83f4910c0000261",
  "user": "f784b56e-e02c-4ede-9d85-fb3ed5566cc5",
  "password":" <some-password>",
  "parameters": [
    {
      "id":"parameter1",
      "type": "float",
      "min":0,
      "max":1000
    },
    {
      "id":"parameter2",
      "type": "float",
      "min":-100,
      "max":100
    }
  ],

  "moe_name": "z",

  "optimization_max_error": 0.0000001,
  "optimization_max_iterations": 16,

  "method_type": "hooke_jeeves",

  "hj_working_step_multiplier": 3,
  "hj_parallel": true,
  "hj_step_sizes": [5, 7],
  "hj_min_step_sizes": [0.0001, 0.0002]
}
```

### PSO algorithm

```json
{
  "address": "system.scalarm.com",
  "experiment_id": "56b1b4f9f83f4910c0000261",
  "user": "f784b56e-e02c-4ede-9d85-fb3ed5566cc5",
  "password":" <some-password>",
  "parameters": [
    {
      "id":"parameter1",
      "type": "float",
      "min":0,
      "max":1000
    },
    {
      "id":"parameter2",
      "type": "float",
      "min":-100,
      "max":100
    }
  ],

  "moe_name": "z",

  "optimization_max_error": 0.0000001,
  "optimization_max_iterations": 16,

  "method_type": "pso",

  "pso_particles_count": 5
}
```

# Output

Depending on the configuration, results will be both send to Scalarm and printed on stdout, or only printed on stout (when ``fake_experiment: true``).

The output is a JSON.

## Output JSON keys

* ``method_type`` - **string**, the same as provided in configuration
* ``step`` - **integer**, value of ``OptimumEventArgs.Step`` on ``EndOfCalculation`` event (Optimization library)
* ``eval_execution_count`` - **integer**, value of ``OptimumEventArgs.EvalExecutionCount`` on ``EndOfCalculation`` event (Optimization library)
* ``parameters`` - **array of floats**, input values of found optimum point; value of ``OptimumEventArgs.Point.Input`` on ``EndOfCalculation`` event (Optimization library)
* ``global_error`` - **float**, final value of found optimum; value of ``OptimumEventArgs.Point.Result`` on ``EndOfCalculation`` event (Optimization library)

## Output examples

### Optimization with genetic algorithm results

```
{
    "method_type": "genetic",
    "step": 0,
    "eval_execution_count": 10,
    "parameters": [
        -9.25615326466791,
        -17.365265710915097
    ],
    "values": [
        -3490.9059587309985
    ],
    "global_error": -3490.9059587309985
}
```

# Optimization intermediate results - state notification

The optimizer sends state update to Scalarm on few optimization events:
- next_step
- new_optimum_found
- end_of_calculations (one-time)

State updates are send to ``<scalarm_base_url>/experiments/<experiment_id>/supervisor_run/state_history``
and history of states can be fetched with ``GET <scalarm_base_url>/experiments/<experiment_id>/supervisor_run/state_history``.

## Example of optimizer state history

```
[{"time":"2016-10-04T11:36:00.178Z","state":{"event_type":"new_optimum_found","moe":4.15193397158,"iteration":0,"evaluations_count":10}},{"time":"2016-10-04T11:36:00.198Z","state":{"event_type":"next_step","iteration":1,"evaluations_count":10}},{"time":"2016-10-04T11:36:23.221Z","state":{"event_type":"new_optimum_found","moe":2.31983742709,"iteration":1,"evaluations_count":20}},{"time":"2016-10-04T11:36:23.241Z","state":{"event_type":"next_step","iteration":2,"evaluations_count":20}},{"time":"2016-10-04T11:36:44.394Z","state":{"event_type":"new_optimum_found","moe":1.0,"iteration":2,"evaluations_count":30}},{"time":"2016-10-04T11:36:44.415Z","state":{"event_type":"next_step","iteration":3,"evaluations_count":30}},{"time":"2016-10-04T11:37:05.557Z","state":{"event_type":"next_step","iteration":4,"evaluations_count":40}},{"time":"2016-10-04T11:37:27.622Z","state":{"event_type":"next_step","iteration":5,"evaluations_count":50}},{"time":"2016-10-04T11:37:48.860Z","state":{"event_type":"next_step","iteration":6,"evaluations_count":60}},{"time":"2016-10-04T11:38:10.399Z","state":{"event_type":"next_step","iteration":7,"evaluations_count":70}},{"time":"2016-10-04T11:38:31.658Z","state":{"event_type":"next_step","iteration":8,"evaluations_count":80}},{"time":"2016-10-04T11:38:53.111Z","state":{"event_type":"next_step","iteration":9,"evaluations_count":90}},{"time":"2016-10-04T11:39:14.402Z","state":{"event_type":"next_step","iteration":10,"evaluations_count":100}},{"time":"2016-10-04T11:39:36.255Z","state":{"event_type":"next_step","iteration":11,"evaluations_count":110}},{"time":"2016-10-04T11:39:59.384Z","state":{"event_type":"next_step","iteration":12,"evaluations_count":120}},{"time":"2016-10-04T11:40:20.928Z","state":{"event_type":"next_step","iteration":13,"evaluations_count":130}},{"time":"2016-10-04T11:40:42.783Z","state":{"event_type":"next_step","iteration":14,"evaluations_count":140}},{"time":"2016-10-04T11:41:04.148Z","state":{"event_type":"next_step","iteration":15,"evaluations_count":150}},{"time":"2016-10-04T11:41:27.948Z","state":{"event_type":"next_step","iteration":16,"evaluations_count":160}},{"time":"2016-10-04T11:41:49.711Z","state":{"event_type":"next_step","iteration":17,"evaluations_count":170}},{"time":"2016-10-04T11:42:13.436Z","state":{"event_type":"next_step","iteration":18,"evaluations_count":180}},{"time":"2016-10-04T11:42:35.111Z","state":{"event_type":"next_step","iteration":19,"evaluations_count":190}},{"time":"2016-10-04T11:42:56.957Z","state":{"event_type":"next_step","iteration":20,"evaluations_count":200}},{"time":"2016-10-04T11:43:18.927Z","state":{"event_type":"end_of_calculations","moe":1.0,"iteration":20,"evaluations_count":210}}] 
```
