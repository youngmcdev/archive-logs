# Patterns used in this project

## Factory

Used for instantiating an object that is an implementation of an abstract type or interface. Allows the object type to be chosen conditionally.

In this project: Generates `System.Commandline` Command and Option objects.

### Factory Components

- Abstract Target: A single interface that represents multiple, similar classes that need to be created. 
  - `CommandLine.Option<T>`
- Concrete Targets: Specific implementations of the Abstract Target which are instantiated by the factories. 
  - `CommandLine.Option<bool>`
  - `CommandLine.Option<T>` where `T` is specified at injection
- Abstract Factory: A single interface having a _creation method_ which returns the Abstract Target.
  - `CliOptions.BaseCliOptionFactory`
- Concrete Factories: Specific implementations of the Abstract Factory that instantiate specific, Concrete Targets via the _creation method_.
  - `CliOptions.BoolCliOptionFactory`
  - `CliOptions.CliOptionFactory`

#### Some Benefits

- Tight coupling between the Factory and the Concrete Target is avoided.
- Open/Closed Principle: New implementation of Abstract Target may be introduced without breaking existing client code.
- Single Responsibility Principle. The logic that creates a Concrete Target may reside in one place, and that's all it does.

## Command

Used to isolated an operation in its own object. 

In this project...

1. Gathers the files in a directory to be archived.
2. Executes 7zip in a process.

### Command Components

- Abstract Command: A single interface that describes the behavior each command should have. Often there is one method that executes _the action_.
  - `Commands.ICommand`
  - `Commands.BaseArchiveCommand`
- Concrete Command: Specific implementations of the Abstract Command. Each accepts paramaters in its constructor and implements the method that executes _the action_.
  - `Commands.ArchiveBuildSourceCommand`
  - `Command.ArchiveFilesCommand`
- Receiver: Class which contains the business logic for the commands. It _knows how_ to complete the operations for the commands.
  - `Commands.ArchiveActions`
- Invoker: Class used to select which commands to execute.
  - `Commands.ArchiveInvoker`

#### Some Benefits
- Single Responsibility Principle: 
  - The operation is fully separated from the objects it operates on.
  - Classes that invoke operations may be decoupled from classes that perform these operations.
- Open/Closed Principle: A new operation may be added in a separate class without modifying any existing code.
- Provides a way to implement deferred execution of operations.
- Combines nicely with Chain of Responsibility to create a complex operation out of multiple simple commands.

## Strategy

Used to encapsulate logic that is executed conditionally, say in an _if_ or _switch_ statement. Each block of logic is moved to its own class.

In this project: Determines the logic that's used to verify whether each file should be included in the archive. This differs based on the format of the log file name.

### Strategy Components

- Abstract Strategy: Single interface that defines the _action method_. It may or may not return data.
  - `Strategies.IArchiveVerifyFileStrategy`
- Concrete Strategy: Specific implementations of the Abstract Strategy. The _strategy to be used_ is set on the Context object based on some conditional logic. 
  - `Strategies.ArchiveVerifyFileStrategy_yyMMdd`
  - `Strategies.ArchiveVerifyFileStrategy_yyyy_MM_dd`
  - `Strategies.ArchiveVerifyFileStrategy_yyyyMM`
  - `Strategies.ArchiveVerifyFileStrategy_yyyyMMdd`
- Context: Should encapsulate the Strategy interface. Allows the calling code to... 
  1. Set its reference (Abstract Strategy) to a Concrete Strategy implementation. 
  2. Execute the _action method_ on the current implementation of the Abstract Strategy.
  - `Services.ArchiveVerifyFileService`

#### Some Benefits
- Reduction of complex, conditional logic as each type of behavior is handled by its own strategy. 
- Algorihtms can be swapped during runtime.
- The Context retains its Strategy potentially reducing the number of times conditional logic must be executed.
- Open/Closed Principle: New strategies may be introduced without having to change the Context.

## Template

Uses a set of protected methods in an abstract class where some methods are abstract and some are implemented. This abstract class is the template. Each class derived from the template is a unique variation having its own logic for the abstract methods.

In this project: Used to define the different Strategies.

### Template Components

- Abstract Template: An abstract class with a non-abstract public method which calls one or more abstract protected methods.
  - `Strategies.ArchiveVerifyFileStrategy`
- Concrete Templates: Classes derived from the Abstract Template overriding the abstract methods giving them implementation-specific behavior.
  - `Strategies.ArchiveVerifyFileStrategy_yyMMdd`
  - `Strategies.ArchiveVerifyFileStrategy_yyyy_MM_dd`
  - `Strategies.ArchiveVerifyFileStrategy_yyyyMM`
  - `Strategies.ArchiveVerifyFileStrategy_yyyyMMdd`
- Client: 
  - `Commands.ArchiveActions`

#### Some Benefits
- Common/duplicated code can be _moved up_ to a superclass.
- Provides a way to define a structure for related algorithms.
- Open/Closed Principle: Original class does not have to be changed to add new behavior.