"""HeAgent exception hierarchy."""


class HeAgentError(Exception):
    """Base exception for all HeAgent errors."""

    def __init__(self, message: str = "") -> None:
        self.message = message
        super().__init__(message)


class ProviderError(HeAgentError):
    """Error from a provider (API call failure, rate limit, etc.)."""


class ToolError(HeAgentError):
    """Error during tool execution."""


class SafetyViolation(HeAgentError):
    """A tool call was blocked by safety guardrails."""


class BudgetExceeded(HeAgentError):
    """Iteration or token budget has been exceeded."""
