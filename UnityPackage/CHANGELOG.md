# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.1] - Unreleased

### Added
- Core blackboard system with key-value storage
- Support for primitive types: int, float, bool, string
- Support for Unity types: Vector3, GameObject, Transform
- Support for List<T> collections for all supported types
- Parent blackboard inheritance for hierarchical value lookup
- Change notification system via Subscribe/Unsubscribe
- Custom inspector for ScriptableObject blackboard assets
- Blackboard debug window for runtime inspection and editing
- Full undo/redo support in Unity editor
- Serialization support for all value types
