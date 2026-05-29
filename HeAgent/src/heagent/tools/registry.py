"""Tool registry — singleton managing all registered tools."""

from __future__ import annotations

from heagent.types import ToolSchema


class ToolRegistry:
    _instance: ToolRegistry | None = None

    def __init__(self) -> None:
        self._tools: dict[str, ToolSchema] = {}
        self._handlers: dict[str, object] = {}
        self._disabled: set[str] = set()

    @classmethod
    def get(cls) -> ToolRegistry:
        if cls._instance is None:
            cls._instance = cls()
        return cls._instance

    @classmethod
    def reset(cls) -> None:
        cls._instance = None

    def register(self, schema: ToolSchema, handler: object) -> None:
        self._tools[schema.name] = schema
        self._handlers[schema.name] = handler
        self._disabled.discard(schema.name)

    def unregister(self, name: str) -> None:
        self._tools.pop(name, None)
        self._handlers.pop(name, None)
        self._disabled.discard(name)

    def get_schema(self, name: str) -> ToolSchema | None:
        return self._tools.get(name)

    def get_handler(self, name: str) -> object | None:
        return self._handlers.get(name)

    def enable(self, name: str) -> None:
        self._disabled.discard(name)

    def disable(self, name: str) -> None:
        if name in self._tools:
            self._disabled.add(name)

    def is_enabled(self, name: str) -> bool:
        return name in self._tools and name not in self._disabled

    def all_schemas(self) -> list[ToolSchema]:
        return list(self._tools.values())

    def enabled_schemas(self) -> list[ToolSchema]:
        return [s for s in self._tools.values() if s.name not in self._disabled]

    def list_names(self) -> list[str]:
        return list(self._tools.keys())
