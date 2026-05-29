"""Tests for @tool decorator and ToolRegistry."""

from __future__ import annotations

import pytest

from heagent.tools.decorator import tool
from heagent.tools.registry import ToolRegistry
from heagent.types import ToolSchema


class TestToolDecorator:
    def test_bare_decorator_registers_tool(self) -> None:
        @tool
        def greet(name: str) -> str:
            """Say hello."""
            return f"Hello, {name}!"

        reg = ToolRegistry.get()
        assert "greet" in reg.list_names()
        schema = reg.get_schema("greet")
        assert schema is not None
        assert schema.description == "Say hello."
        assert schema.parameters["required"] == ["name"]
        reg.unregister("greet")

    def test_decorator_with_args(self) -> None:
        @tool(name="custom_name_test", description="Custom desc")
        def my_func(x: int) -> str:
            return str(x)

        reg = ToolRegistry.get()
        assert "custom_name_test" in reg.list_names()
        schema = reg.get_schema("custom_name_test")
        assert schema is not None
        assert schema.description == "Custom desc"
        reg.unregister("custom_name_test")

    def test_type_mapping(self) -> None:
        @tool
        def typed(a: str, b: int, c: float, d: bool) -> str:
            """Typed func."""
            return ""

        schema = ToolRegistry.get().get_schema("typed")
        assert schema is not None
        props = schema.parameters["properties"]
        assert isinstance(props, dict)
        assert props["a"]["type"] == "string"
        assert props["b"]["type"] == "integer"
        assert props["c"]["type"] == "number"
        assert props["d"]["type"] == "boolean"
        ToolRegistry.get().unregister("typed")

    def test_optional_params_not_required(self) -> None:
        @tool
        def opt(name: str, count: int = 5) -> str:
            """Optional params."""
            return ""

        schema = ToolRegistry.get().get_schema("opt")
        assert schema is not None
        assert schema.parameters["required"] == ["name"]
        props = schema.parameters["properties"]
        assert isinstance(props, dict)
        assert props["count"]["default"] == 5
        ToolRegistry.get().unregister("opt")

    def test_disabled_tool(self) -> None:
        @tool(enabled=False)
        def secret(x: str) -> str:
            """Secret tool."""
            return ""

        reg = ToolRegistry.get()
        assert "secret" in reg.list_names()
        assert not reg.is_enabled("secret")
        assert reg.get_schema("secret") is not None
        reg.unregister("secret")

    def test_handler_stored(self) -> None:
        @tool
        def compute(n: int) -> int:
            """Compute."""
            return n * 2

        handler = ToolRegistry.get().get_handler("compute")
        assert handler is not None
        assert handler(5) == 10
        ToolRegistry.get().unregister("compute")

    def test_original_function_preserved(self) -> None:
        @tool
        def add(a: int, b: int) -> int:
            """Add."""
            return a + b

        assert add(1, 2) == 3
        ToolRegistry.get().unregister("add")


class TestToolRegistry:
    def test_singleton(self) -> None:
        a = ToolRegistry.get()
        b = ToolRegistry.get()
        assert a is b

    def test_register_and_get(self) -> None:
        reg = ToolRegistry.get()
        schema = ToolSchema(name="test", description="test", parameters={"type": "object", "properties": {}})
        handler = lambda: None
        reg.register(schema, handler)

        assert reg.get_schema("test") is schema
        assert reg.get_handler("test") is handler
        reg.unregister("test")

    def test_unregister(self) -> None:
        reg = ToolRegistry.get()
        schema = ToolSchema(name="tmp", description="tmp", parameters={"type": "object", "properties": {}})
        reg.register(schema, lambda: None)
        assert "tmp" in reg.list_names()

        reg.unregister("tmp")
        assert "tmp" not in reg.list_names()
        assert reg.get_schema("tmp") is None

    def test_enable_disable(self) -> None:
        reg = ToolRegistry.get()
        schema = ToolSchema(name="ctrl", description="ctrl", parameters={"type": "object", "properties": {}})
        reg.register(schema, lambda: None)

        assert reg.is_enabled("ctrl")
        reg.disable("ctrl")
        assert not reg.is_enabled("ctrl")
        assert reg.get_schema("ctrl") not in reg.enabled_schemas()
        assert reg.get_schema("ctrl") in reg.all_schemas()

        reg.enable("ctrl")
        assert reg.is_enabled("ctrl")
        assert reg.get_schema("ctrl") in reg.enabled_schemas()
        reg.unregister("ctrl")

    def test_disable_nonexistent_is_noop(self) -> None:
        reg = ToolRegistry.get()
        reg.disable("ghost")  # should not raise
