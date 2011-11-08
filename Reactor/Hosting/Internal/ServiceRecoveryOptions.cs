using Reactor.Configuration;

namespace Reactor.Hosting.Internal
{
    public class ServiceRecoveryOptions
    {
        public ServiceRecoveryOptions()
        {
            FirstFailureAction = ServiceRecoveryAction.TakeNoAction;
            SecondFailureAction = ServiceRecoveryAction.TakeNoAction;
            SubsequentFailureActions = ServiceRecoveryAction.TakeNoAction;
            DaysToResetFailAcount = 0;
            MinutesToRestartService = 1;
        }

        public ServiceRecoveryAction FirstFailureAction { get; set; }
        public ServiceRecoveryAction SecondFailureAction { get; set; }
        public ServiceRecoveryAction SubsequentFailureActions { get; set; }
        public int DaysToResetFailAcount { get; set; }
        public int MinutesToRestartService { get; set; }
        public string RebootMessage { get; set; }
        public string CommandToLaunchOnFailure { get; set; }

        public void Validate()
        {
            Exceptions.ThrowHelper.ThrowInvalidOperationExceptionIf(
                s =>
                !string.IsNullOrEmpty(s.RebootMessage) &&
                !s.RecoveryActionIsDefined(ServiceRecoveryAction.RestartTheComputer),
                this,
                "Setting '{0}' is not valid when there is no '{1}' failure action/s defined.",
                "RebootMessage", ServiceRecoveryAction.RestartTheComputer);
            Exceptions.ThrowHelper.ThrowInvalidOperationExceptionIf(
                s =>
                !string.IsNullOrEmpty(s.CommandToLaunchOnFailure) &&
                !s.RecoveryActionIsDefined(ServiceRecoveryAction.RunAProgram),
                this,
                "Setting '{0}' is not valid when there is no '{1}' failure action/s defined.",
                "CommandToLaunchOnFailure", ServiceRecoveryAction.RunAProgram);
            Exceptions.ThrowHelper.ThrowInvalidOperationExceptionIf(
                s =>
                s.MinutesToRestartService > 1 &&
                !s.RecoveryActionIsDefined(ServiceRecoveryAction.RestartTheService),
                this,
                "Setting '{0}' is not valid when there is no '{1}' failure action/s defined.",
                "MinutesToRestartService", ServiceRecoveryAction.RestartTheService);
        }

        private bool RecoveryActionIsDefined(ServiceRecoveryAction action)
        {
            return FirstFailureAction == action ||
                   SecondFailureAction == action ||
                   SubsequentFailureActions == action;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != typeof(ServiceRecoveryOptions)) return false;
            return Equals((ServiceRecoveryOptions)other);
        }

        public bool Equals(ServiceRecoveryOptions other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.FirstFailureAction, FirstFailureAction) && Equals(other.SecondFailureAction, SecondFailureAction) && Equals(other.SubsequentFailureActions, SubsequentFailureActions) && other.DaysToResetFailAcount == DaysToResetFailAcount && other.MinutesToRestartService == MinutesToRestartService && Equals(other.RebootMessage, RebootMessage) && Equals(other.CommandToLaunchOnFailure, CommandToLaunchOnFailure);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = FirstFailureAction.GetHashCode();
                result = (result * 397) ^ SecondFailureAction.GetHashCode();
                result = (result * 397) ^ SubsequentFailureActions.GetHashCode();
                result = (result * 397) ^ DaysToResetFailAcount;
                result = (result * 397) ^ MinutesToRestartService;
                result = (result * 397) ^ (RebootMessage != null ? RebootMessage.GetHashCode() : 0);
                result = (result * 397) ^ (CommandToLaunchOnFailure != null ? CommandToLaunchOnFailure.GetHashCode() : 0);
                return result;
            }
        }

        public static ServiceRecoveryOptions FromConfiguration(RecoveryOptionsElement recoveryOptionsElement)
        {
            var recoveryOptions = new ServiceRecoveryOptions
                                      {
                                          FirstFailureAction = recoveryOptionsElement.FirstFailureAction,
                                          SecondFailureAction = recoveryOptionsElement.SecondFailureAction,
                                          SubsequentFailureActions = recoveryOptionsElement.SubsequentFailureActions,
                                          DaysToResetFailAcount = recoveryOptionsElement.DaysToResetFailAcount,
                                          CommandToLaunchOnFailure = recoveryOptionsElement.CommandToLaunchOnFailure,
                                          MinutesToRestartService = recoveryOptionsElement.MinutesToRestartService,
                                          RebootMessage = recoveryOptionsElement.RebootMessage
                                      };
            recoveryOptions.Validate();
            return recoveryOptions;
        }
    }
}
