# Формат проекта

Формат хранения — JSON.

Корневой объект:
- version
- name
- nodes[]
- connections[]

Каждый узел содержит:
- id
- kind
- text
- x
- y

Каждая связь содержит:
- id
- fromNodeId
- fromPort
- toNodeId
- toPort
