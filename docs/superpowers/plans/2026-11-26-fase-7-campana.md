# Fase 7: Campaña 2–5 — Plan

> Fase 7 implementada en el proyecto. El commit de la Fase 6 es `caba8ef`; la Fase 7 quedó
> commiteada en `9fb38d9`.

**Objetivo:** Campaña jugable de Nivel 1 a Nivel 5 con objetivos, núcleo final y victoria.

**Spec:** `docs/superpowers/specs/2026-11-26-fase-7-campana-design.md`

## Tareas

- [x] **Controlador de campaña**
  - `LevelController` genérico con objetivo, oleada opcional, núcleo y siguiente escena.
  - Carga automática del siguiente nivel.
  - Mensaje de finalización en el HUD.
  - `ObjectivePoint` para computadora y puerta.
  - `CoreBoss` con oleadas, apertura y 1000 de vida.
  - `VictoryScreen` y escena Victory.

- [x] **Progresión**
  - `PlayerSelection` persistente entre niveles.
  - Selección solo en el Nivel 1.
  - Reinicio de la selección desde Victory.

- [x] **Niveles**
  - Level02 Hospital: tema, etiquetas, corredores, terminal, armas.
  - Level03 Industrial: blindado, escupidor, terminal, armas.
  - Level04 Metro: luz baja, trepador, chillón, oleada final y salida.
  - Level05 Laboratorio: nidificante, mutado, zona oscura, núcleo.
  - Build Settings con Level01–05 y Victory.

- [x] **Verificación**
  - Level01→02 con Marcus y 140 de vida.
  - Level02→03 con Sofía y 90 de vida.
  - Level03→04, Level04→05 y Level05→Victory.
  - Objetivos de terminal y oleadas.
  - Núcleo abierto/dañable; `CAMPAÑA COMPLETADA`; Victory con selección reseteada.
  - Objetivo del Nivel 4 corregido antes de la oleada.
  - Consola limpia.

- [ ] **Pendiente manual**
  - Completar la campaña con Alex, Sofía, Marcus y Luna.
  - Ajustar dificultad, munición y tiempo del núcleo.
  - Sustituir la planta reutilizada y el arte provisional en la Fase 8.
  - Probar la pantalla de victoria con teclado y gamepad.

## Cómo probar

1. Abrir `Level01`, Play y elegir personaje.
2. Completar el Nivel 1 y seguir la campaña por los Niveles 2–5.
3. En el Nivel 2 recuperar la información; en el 3 activar la puerta.
4. En el 4 cruzar el metro con poca luz.
5. En el 5 destruir el núcleo: limpiar oleadas, aprovechar las ventanas vulnerables y llegar a
   Victory.
