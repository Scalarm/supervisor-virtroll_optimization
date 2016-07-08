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

#### Hooke-Jeeves algorithm configuration

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
  "genetic_population_max": 20
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
* ``values`` - **array of floats**, list of results evaluated in each optimization step; value of ``OptimumEventArgs.Point.PartialResults`` on ``EndOfCalculation`` event (Optimization library)
* ``global_value`` - **float**, final value of found optimum; value of ``OptimumEventArgs.Point.Result`` on ``EndOfCalculation`` event (Optimization library)

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
    "global_value": -3490.9059587309985
}
```
