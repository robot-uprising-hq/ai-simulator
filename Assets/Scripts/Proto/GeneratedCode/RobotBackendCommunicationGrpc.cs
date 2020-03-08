// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: RobotBackendCommunication.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Robotbackendcommunication {
  public static partial class RobotBackendCommunicator
  {
    static readonly string __ServiceName = "robotbackendcommunication.RobotBackendCommunicator";

    static readonly grpc::Marshaller<global::Robotbackendcommunication.Screenshot> __Marshaller_robotbackendcommunication_Screenshot = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Robotbackendcommunication.Screenshot.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Robotbackendcommunication.AgentAction> __Marshaller_robotbackendcommunication_AgentAction = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Robotbackendcommunication.AgentAction.Parser.ParseFrom);

    static readonly grpc::Method<global::Robotbackendcommunication.Screenshot, global::Robotbackendcommunication.AgentAction> __Method_GetAction = new grpc::Method<global::Robotbackendcommunication.Screenshot, global::Robotbackendcommunication.AgentAction>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetAction",
        __Marshaller_robotbackendcommunication_Screenshot,
        __Marshaller_robotbackendcommunication_AgentAction);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Robotbackendcommunication.RobotBackendCommunicationReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of RobotBackendCommunicator</summary>
    public abstract partial class RobotBackendCommunicatorBase
    {
      public virtual global::System.Threading.Tasks.Task<global::Robotbackendcommunication.AgentAction> GetAction(global::Robotbackendcommunication.Screenshot request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for RobotBackendCommunicator</summary>
    public partial class RobotBackendCommunicatorClient : grpc::ClientBase<RobotBackendCommunicatorClient>
    {
      /// <summary>Creates a new client for RobotBackendCommunicator</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public RobotBackendCommunicatorClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for RobotBackendCommunicator that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public RobotBackendCommunicatorClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected RobotBackendCommunicatorClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected RobotBackendCommunicatorClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::Robotbackendcommunication.AgentAction GetAction(global::Robotbackendcommunication.Screenshot request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetAction(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Robotbackendcommunication.AgentAction GetAction(global::Robotbackendcommunication.Screenshot request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetAction, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Robotbackendcommunication.AgentAction> GetActionAsync(global::Robotbackendcommunication.Screenshot request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetActionAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Robotbackendcommunication.AgentAction> GetActionAsync(global::Robotbackendcommunication.Screenshot request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetAction, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override RobotBackendCommunicatorClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new RobotBackendCommunicatorClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(RobotBackendCommunicatorBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_GetAction, serviceImpl.GetAction).Build();
    }

  }
}
#endregion
