# 🎮 Estructura del Proyecto: Platformer 2D con Persecución

## 📋 Descripción General

Este proyecto implementa un juego de plataformas 2D basado en el concepto descrito en `main.txt`, donde el jugador debe escapar de un enemigo perseguidor mientras navega por niveles desafiantes usando habilidades como salto en paredes, dash y un estado de furia destructivo.

## 🏗️ Arquitectura del Proyecto

### 📁 Estructura de Carpetas

```
Assets/
├── Scripts/
│   ├── Core/                    # Sistemas centrales
│   │   ├── GameManager.cs       # Gestor principal del juego
│   │   └── LevelManager.cs      # Gestor de niveles y objetivos
│   ├── Player/                  # Sistemas del jugador
│   │   └── PlayerController.cs  # Controlador principal del jugador
│   ├── Enemies/                 # Sistemas de enemigos
│   │   ├── ChaserEnemy.cs      # Enemigo perseguidor principal
│   │   ├── TurretAI.cs         # IA de torretas
│   │   └── TurretProjectile.cs # Proyectiles de torretas
│   └── Environment/             # Elementos del entorno
│       ├── FragileWall.cs      # Paredes destructibles
│       ├── BouncePad.cs        # Trampolines
│       └── ObjetoDestructible.cs # Sistema base de objetos dañables
├── Prefabs/                     # Prefabs del juego
├── Scenes/                      # Escenas del juego
└── Sprites/                     # Recursos gráficos
```

## 🔧 Componentes Principales

### 🎯 GameManager (Singleton)
**Ubicación:** `Assets/Scripts/Core/GameManager.cs`

**Funciones principales:**
- Control de estados del juego (Menu, Jugando, Pausado, GameOver, Victoria)
- Gestión de vidas y tiempo límite
- Sistema de eventos globales
- Persistencia entre escenas

**Configuración en Unity:**
1. Crear GameObject vacío llamado "GameManager"
2. Agregar script `GameManager`
3. Configurar valores en el inspector:
   - `tiempoLimiteNivel`: 300 segundos (5 minutos)
   - `vidasIniciales`: 3
   - `modoDebug`: true para desarrollo

### 🏃‍♂️ PlayerController
**Ubicación:** `Assets/Scripts/Player/PlayerController.cs`

**Mecánicas implementadas:**
- **Movimiento básico:** WASD o flechas
- **Salto:** Espacio (incluye salto en paredes)
- **Dash:** Shift izquierdo (con cooldown)
- **Estado de Furia:** F (permite romper paredes)
- **Agacharse:** S o flecha abajo

**Configuración en Unity:**
1. Crear GameObject "Player"
2. Agregar componentes:
   - `SpriteRenderer`
   - `Rigidbody2D` (gravityScale = 0)
   - `CapsuleCollider2D`
   - Script `PlayerController`
3. Crear GameObject hijo "GroundCheck" para verificación de suelo
4. Configurar capas de colisión

### 👹 ChaserEnemy (Perseguidor)
**Ubicación:** `Assets/Scripts/Enemies/ChaserEnemy.cs`

**Comportamientos:**
- **Buscando:** Patrulla aleatoriamente
- **Persiguiendo:** Sigue al jugador activamente
- **Huyendo:** Huye cuando el jugador está en furia
- **Buscando última posición:** Va al último lugar donde vio al jugador

**Configuración en Unity:**
1. Crear GameObject "ChaserEnemy"
2. Agregar componentes:
   - `NavMeshAgent` (configurado para 2D)
   - `Rigidbody2D`
   - `SpriteRenderer`
   - `Collider2D` con trigger activado
   - Script `ChaserEnemy`
3. Configurar tag "Deadly" para eliminar al jugador
4. Crear NavMesh 2D en la escena

### 🔫 TurretAI (Torretas)
**Ubicación:** `Assets/Scripts/Enemies/TurretAI.cs`

**Funcionalidades:**
- Detección automática del jugador
- Disparo de proyectiles con cooldown
- Rotación hacia el objetivo
- Sistema de munición limitada/ilimitada

**Configuración en Unity:**
1. Crear GameObject "Turret"
2. Agregar script `TurretAI`
3. Crear prefab de proyectil y asignarlo
4. Configurar punto de disparo y parte rotatoria

### 🧱 FragileWall (Paredes Destructibles)
**Ubicación:** `Assets/Scripts/Environment/FragileWall.cs`

**Métodos de destrucción:**
- Jugador en estado de furia
- Proyectiles de torretas
- Explosiones

**Configuración en Unity:**
1. Crear GameObject con sprite de pared
2. Agregar `Collider2D` como trigger
3. Agregar script `FragileWall`
4. Configurar tag "FragileWall"

### 🚀 BouncePad (Trampolines)
**Ubicación:** `Assets/Scripts/Environment/BouncePad.cs`

**Características:**
- Impulso configurable en dirección y fuerza
- Cooldown de uso
- Efectos visuales y sonoros
- Solo afecta al jugador por defecto

### 🎯 LevelManager (Gestor de Niveles)
**Ubicación:** `Assets/Scripts/Core/LevelManager.cs`

**Tipos de objetivos:**
- Alcanzar meta
- Recolectar llaves
- Eliminar enemigos
- Activar puntos de control
- Supervivencia por tiempo
- Objetivos combinados

## 🎮 Controles del Juego

| Acción | Tecla |
|--------|-------|
| Movimiento | WASD o Flechas |
| Salto | Espacio |
| Dash | Shift Izquierdo |
| Estado de Furia | F |
| Agacharse | S o Flecha Abajo |
| Pausa | ESC |

## ⚙️ Configuración de Unity

### 🏷️ Tags Necesarios
- `Player`: Jugador normal
- `PlayerFury`: Jugador en estado de furia
- `Enemy`: Enemigos generales
- `Deadly`: Objetos que matan al jugador
- `Wall`: Paredes normales
- `FragileWall`: Paredes destructibles
- `Projectile`: Proyectiles
- `Explosion`: Áreas de explosión
- `Goal`: Meta del nivel
- `Key`: Llaves coleccionables
- `ControlPoint`: Puntos de control

### 🎯 Capas (Layers)
- `Default`: Objetos generales
- `Player`: Jugador
- `Enemies`: Enemigos
- `Walls`: Paredes y obstáculos
- `Projectiles`: Proyectiles
- `UI`: Interfaz de usuario

### ⚙️ Configuración de Física 2D
1. **Project Settings > Physics 2D**
2. Configurar **Layer Collision Matrix**:
   - Player no colisiona con Enemies (excepto triggers)
   - Projectiles colisionan con Walls y FragileWalls
   - Enemies colisionan con Walls

### 🗺️ NavMesh 2D
1. Instalar **NavMeshComponents** desde Package Manager
2. Agregar **NavMeshSurface** al GameObject raíz de la escena
3. Configurar para 2D (Agent Type: Humanoid, Override Area: Walkable)
4. Marcar suelos como **Navigation Static**
5. Hacer "Bake" del NavMesh

## 🎨 Setup de Escena Básica

### 1. Crear Escena Base
```
Escena "Nivel1"
├── GameManager (Prefab)
├── LevelManager
├── Player (Prefab)
├── ChaserEnemy (Prefab)
├── Environment/
│   ├── Walls (Sprites con Colliders)
│   ├── FragileWalls (Prefabs)
│   ├── BouncePads (Prefabs)
│   └── Spikes (Tag: Deadly)
├── Enemies/
│   └── Turrets (Prefabs)
├── Collectibles/
│   └── Keys (Tag: Key)
└── Goal (Tag: Goal)
```

### 2. Configurar Cámara
- **Projection:** Orthographic
- **Size:** 5-8 (ajustar según necesidad)
- Agregar script de seguimiento de cámara (opcional)

### 3. Configurar Iluminación 2D
- Agregar **Global Light 2D**
- Configurar **Universal Render Pipeline 2D**

## 🚀 Flujo de Implementación

### Fase 1: Setup Básico
1. ✅ Crear estructura de carpetas
2. ✅ Implementar GameManager
3. ✅ Crear PlayerController básico
4. ✅ Configurar física 2D

### Fase 2: Mecánicas Core
1. ✅ Implementar movimiento y salto
2. ✅ Agregar dash y estado de furia
3. ✅ Crear sistema de paredes destructibles
4. ✅ Implementar trampolines

### Fase 3: Enemigos y IA
1. ✅ Crear enemigo perseguidor
2. ✅ Implementar torretas
3. ✅ Sistema de proyectiles
4. ✅ NavMesh 2D

### Fase 4: Gestión de Niveles
1. ✅ LevelManager con objetivos
2. ✅ Sistema de llaves y metas
3. ✅ Puntos de control
4. ✅ Condiciones de victoria/derrota

### Fase 5: Polish y Efectos
1. 🔄 Efectos visuales y sonoros
2. 🔄 Animaciones
3. 🔄 UI y HUD
4. 🔄 Menús y transiciones

## 🔧 Scripts de Utilidad

### Configuración Rápida de Player
```csharp
// En el inspector del PlayerController:
velocidadMovimiento = 8f
fuerzaSalto = 16f
fuerzaDash = 24f
duracionFuria = 5f
```

### Configuración Rápida de ChaserEnemy
```csharp
// En el inspector del ChaserEnemy:
velocidadNormal = 3f
velocidadEscape = 5f
distanciaDeteccion = 15f
distanciaMinima = 1.5f
```

## 🐛 Troubleshooting

### Problemas Comunes

1. **El jugador no salta en paredes**
   - Verificar LayerMask `capasParedes`
   - Asegurar que las paredes tengan Collider2D

2. **El perseguidor no sigue al jugador**
   - Verificar que el NavMesh esté "baked"
   - Comprobar que el NavMeshAgent esté configurado para 2D

3. **Las paredes frágiles no se destruyen**
   - Verificar tags correctos
   - Asegurar que los Colliders sean triggers

4. **Los proyectiles no funcionan**
   - Verificar prefab del proyectil
   - Comprobar configuración de capas de colisión

## 📝 Notas de Desarrollo

- **Rendimiento:** Usar Object Pooling para proyectiles en niveles complejos
- **Escalabilidad:** El sistema de eventos permite agregar nuevas mecánicas fácilmente
- **Modularidad:** Cada script es independiente y reutilizable
- **Debug:** Activar `modoDebug` en GameManager para información adicional

## 🎯 Próximos Pasos

1. Implementar sistema de sonido completo
2. Crear más tipos de enemigos
3. Agregar power-ups y mejoras
4. Sistema de puntuación
5. Múltiples niveles con dificultad progresiva
6. Efectos de partículas avanzados
7. Sistema de guardado/carga

---

**¡El proyecto está listo para ser implementado siguiendo esta estructura!** 🚀 