# Fase 6: Personajes — Diseño

## Objetivo

Añadir Sofía, Marcus y Luna, una selección de personaje y las habilidades que les permitan
completar el Nivel 1 junto a Alex.

## Enfoque

- Un prefab de jugador compartido y `CharacterData` por personaje, aplicado al empezar el Nivel 1.
- La selección es una capa runtime sobre el inicio de `Level01` (sin escena nueva todavía); una
  pantalla de selección dedicada y el menú principal quedan para la Fase 8.
- Los datos de balance viven en `PlayerConfig` y `CharacterData`.
- Entrada de habilidad nueva: `E` / `X`.

## Personajes

| Personaje | Vida | Velocidad | Arma inicial | Habilidad |
| --- | ---: | ---: | --- | --- |
| Alex | 100 | 6.5 | Pistola | +25 % de munición. |
| Sofía | 90 | 7.0 | Pistola | Se cura 25 cada 45 s. |
| Marcus | 140 | 5.5 | Escopeta | Embestida con escudo (E): avanza, bloquea daño y atraviesa enemigos. |
| Luna | 75 | 7.5 | Ballesta ∞ | Doble salto y cuchillo (E) cuerpo a cuerpo. |

## Implementación

- `CharacterData` referencia configuración, sprites de pie/agachado, arma inicial y habilidad.
- `PlayerCharacterApplier` aplica configuración, vida, arma, sprites, saltos y `PlayerAbilities`.
- `PlayerAbilities` gestiona curación, escudo y cuchillo.
- `PlayerMotor` añade salto aéreo, embestida y reconfiguración.
- `PlayerHealth` añade reconfiguración e invulnerabilidad temporal.
- `PlayerShooter` añade multiplicador de munición y reconfiguración.
- `CharacterSelectUI` construye la pantalla de selección en runtime y navega con teclado/gamepad.
- Los assets están en `Resources/Characters` para cargarse sin referencias de escena.

## Verificación

- Los cuatro personajes se cargan y se pueden seleccionar.
- Alex: 100 de vida, pistola, 38 cartuchos de escopeta al recogerla con el bonus.
- Sofía: 90 de vida, cura de 70 a 90 con el temporizador de prueba.
- Marcus: 140 de vida, escopeta, embestida con escudo y shield visual.
- Luna: 75 de vida, ballesta infinita, salto aéreo y cuchillo (30 → 10 de vida a un común).
- Compilación y consola limpias.

## Pendiente

- Pasada completa del Nivel 1 con cada personaje (prueba manual de 4–6 minutos por personaje).
- Pantalla de selección dedicada/Menú principal en la Fase 8.
- Retratos, arte final y audio.
