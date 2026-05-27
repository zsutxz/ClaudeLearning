"""Tests for HeAgent exception hierarchy."""

from heagent.exceptions import (
    BudgetExceeded,
    HeAgentError,
    ProviderError,
    SafetyViolation,
    ToolError,
)


def test_heagent_error_message() -> None:
    err = HeAgentError("test error")
    assert err.message == "test error"
    assert str(err) == "test error"


def test_provider_error_inherits() -> None:
    err = ProviderError("api failed")
    assert isinstance(err, HeAgentError)
    assert err.message == "api failed"


def test_tool_error_inherits() -> None:
    err = ToolError("execution failed")
    assert isinstance(err, HeAgentError)


def test_safety_violation_inherits() -> None:
    err = SafetyViolation("dangerous command")
    assert isinstance(err, HeAgentError)


def test_budget_exceeded_inherits() -> None:
    err = BudgetExceeded("max iterations reached")
    assert isinstance(err, HeAgentError)
