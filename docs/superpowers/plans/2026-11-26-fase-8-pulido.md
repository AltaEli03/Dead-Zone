# Fase 8: Pulido — Plan

> Fase 8 en curso. La Fase 7 quedó commiteada en `9fb38d9`; los cambios de Pulido aún no.

**Objetivo:** Menú principal, selección dedicada, pausa, derrota, opciones de audio, límite de 60 FPS y build de Windows jugable.

## Tareas

- [x] **Menú principal**
  - Escena `MainMenu` con JUGAR, OPCIONES y SALIR.
  - Navegación con teclado y gamepad.
  - Volumen maestro ajustable y guardado en `PlayerPrefs`.

- [x] **Selección dedicada**
  - Escena `CharacterSelect` antes del Nivel 1.
  - Los cuatro personajes con vista previa y estadísticas.
  - La selección persiste entre niveles y se reinicia al volver al menú.

- [x] **Pausa y derrota**
  - Pausa con Reanudar, Opciones, Reiniciar nivel y Menú principal.
  - Derrota con Reintentar y Menú principal.
  - `Time.timeScale` y audio en pausa controlados.
  - Eliminado el mensaje duplicado de GAME OVER del HUD.

- [x] **Audio provisional**
  - `AudioManager` persistente con efectos de disparo, impacto, explosión, daño, curación, recogida, zombi, grito y núcleo.
  - Música de menú, niveles y victoria; el tono sube ligeramente por nivel.
  - `GameSettings` aplica volumen maestro y 60 FPS.

- [x] **Build de Windows**
  - Build Settings: MainMenu, CharacterSelect, Level01–05 y Victory.
  - `Builds/Windows/DeadZone.exe` generada con 0 errores.
  - El ejecutable abre el menú y el log no muestra errores.

- [x] **Verificación**
  - MainMenu → CharacterSelect → Level01 con Alex.
  - Pausa y derrota revisadas en el editor.
  - Menú, selección y pausa capturados; consola del editor limpia.

- [ ] **Pendiente manual**
  - Completar la campaña con Alex, Sofía, Marcus y Luna en la build.
  - Ajustar dificultad, munición y tiempo del núcleo.
  - Textos de historia antes de cada nivel y guardado del nivel desbloqueado.
  - Sustituir el arte provisional y la planta reutilizada.

## Cómo probar

1. Abrir `Builds/Windows/DeadZone.exe`.
2. Elegir JUGAR, seleccionar un superviviente y jugar el Nivel 1.
3. Pausar con Esc o Start, probar Opciones, Reanudar y Reiniciar nivel.
4. Completar la campaña hasta destruir el núcleo y volver al menú.
