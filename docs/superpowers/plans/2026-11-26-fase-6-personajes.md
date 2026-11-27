# Fase 6: Personajes — Plan

> Trabajo de la Fase 6. El commit de la Fase 5 está hecho (`044a79a`); este trabajo aún no se ha
> commiteado.

**Objetivo:** Cuatro personajes jugables, selección y habilidades, capaces de jugar el Nivel 1.

**Spec:** `docs/superpowers/specs/2026-11-26-fase-6-personajes-design.md`

## Tareas

- [x] **Datos y configuración**
  - `CharacterData` y assets de Alex, Sofía, Marcus y Luna.
  - `SofiaConfig`, `MarcusConfig`, `LunaConfig`.
  - Arma `Ballesta` infinita y sprite `ArmCrossbow`.
  - Sprites de cuerpo y agachado de los tres personajes nuevos.

- [x] **Jugador**
  - `PlayerCharacterApplier` y `PlayerSelection`.
  - `PlayerMotor.Configure`, salto aéreo y embestida.
  - `PlayerHealth.Configure` e invulnerabilidad temporal.
  - `PlayerShooter.Configure` y multiplicador de munición.
  - `PlayerAbilities` (curación, escudo, cuchillo).

- [x] **Selección**
  - `CharacterSelectUI` runtime con previsualización, estadísticas y habilidad.
  - Integración en `GameManager` al inicio del Nivel 1.
  - Pausa, desactivación del input del jugador y reanudación al confirmar.

- [x] **Entrada y UI**
  - Acción `Ability` en `E` / `X`.
  - `AbilityPressed` en `PlayerInputReader`.
  - Texto de controles y README actualizados.

- [x] **Verificación**
  - Carga de los cuatro personajes.
  - Alex: bonus de munición.
  - Sofía: curación.
  - Marcus: escopeta y escudo.
  - Luna: ballesta, doble salto y cuchillo.
  - Consola y compilación limpias.

- [ ] **Pendiente manual**
  - Completar el Nivel 1 con cada uno de los cuatro personajes.
  - Revisar sensación de la selección con teclado y gamepad reales.
  - Ajustar balance tras la pasada completa.

## Cómo probar

1. Abrir `Assets/_Project/Scenes/Level01.unity` y Play.
2. Elegir con A/D o stick y confirmar con J/Espacio/A.
3. Probar la habilidad con E/X:
   - Sofía cura sola.
   - Marcus embiste con escudo.
   - Luna salta dos veces y ataca con el cuchillo.
4. Completar el nivel con cada personaje.
