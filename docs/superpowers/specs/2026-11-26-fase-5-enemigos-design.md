# Fase 5: Enemigos — Diseño

## Objetivo

Añadir los siete enemigos restantes de la tabla de diseño y las crías del Nidificante, cada uno
probado en una escena de pruebas. La fase no integra todavía estos enemigos en los niveles 2–5 ni
sustituye el arte provisional.

## Alcance

- Enemigos: Corredor, Trepador, Escupidor, Chillón, Blindado, Nidificante, Mutado y Cría.
- Escena nueva `Test_Phase05`, copiada de `Test_Phase01` para conservar jugador, cámara, HUD,
  armas y objetos.
- `Test_Phase01` y `Level01` no cambian de contenido; `Level01` sigue usando solo zombis comunes.
- Sin audio, menús, arte final ni integración de niveles.

## Arquitectura

- `EnemyAgent` concentra objetivo, gravedad, muerte, caída y ataque cuerpo a cuerpo.
- `ZombieChase` hereda de `EnemyAgent` y mantiene el comportamiento del común. Los enemigos que
  comparten persecución heredan de `ZombieChase`; los de comportamiento propio heredan de
  `EnemyAgent`.
- `EnemyData` guarda toda la configuración de balance, con un `EnemyKind` y campos agrupados por
  comportamiento.
- `DamageInfo` añade origen y `bypassShield` al daño. `IDamageable` mantiene la sobrecarga antigua
  para compatibilidad.
- `EnemyHealth` añade `Heal` y el evento `Died`. El Blindado usa un componente `Shield`.
- `PlayerVisibility` y `DarknessZone` permiten probar la revelación del Mutado sin depender todavía
  de la iluminación de los niveles 4–5.

## Comportamientos

| Enemigo | Vida | Velocidad | Comportamiento |
| --- | ---: | ---: | --- |
| Corredor | 25 | 4.2 | Espera hasta que el jugador se acerca; persecución rápida y salto en huecos o paredes. |
| Trepador | 35 | 2.6 | Se agarra a una pared con gravedad cero; trepa hacia arriba si el jugador está arriba y cae cuando pasa por debajo. |
| Escupidor | 60 | 1.4 | Mantiene distancia, dispara ácido en arco y deja un charco dañino. |
| Chillón | 40 | 0 | Se queda quieto, grita, alerta y genera dos zombis comunes. |
| Blindado | 200 | 1.2 | Gira lento; el escudo bloquea daño frontal no explosivo. Espalda y explosivos lo dañan. |
| Nidificante | 180 | 1.2 | Cura a enemigos cercanos y libera tres crías al morir. |
| Mutado | 350 | 2.0 | Movimiento errático, luz verde y revela al jugador oculto si está cerca. |
| Cría | 10 | 3.4 | Persecución rápida y cuerpo a cuerpo; escala 0.55. |

## Escena de pruebas

`Test_Phase05` usa el lado derecho vacío de `Test_Phase01`:

- Pared izquierda nueva para el Trepador, junto al punto de inicio.
- Corredor, Blindado, Nidificante y un común en la plataforma central.
- Chillón en el escalón.
- Escupidor, Mutado y un común dentro de una zona oscura de prueba en la plataforma alta.
- Etiquetas TMP sobre cada enemigo y sobre la zona oscura.
- El spawner común queda desactivado al inicio; el HUD lo sigue usando como referencia.

## Pruebas

- Compilación y consola limpias.
- El común conserva persecución, daño y muerte.
- Escudo: 10 de daño frontal no resta vida; por la espalda resta 10; explosivo resta 10.
- Nidificante: cura a un común dañado; al morir aparecen tres crías de 10 de vida.
- Chillón: invoca dos zombis comunes.
- Trepador: se mantiene en la pared con gravedad 0 y cae al acercarse el jugador.
- Charco de ácido: daña al jugador por ticks.
- Zona oscura: oculta al jugador; el Mutado cercano lo revela; al salir deja de estar oculto.
- Corredor: despierta y ataca al acercarse el jugador.

## Atajos y riesgos

- El Trepador es un emboscador de pared con trepa básica, no navegación completa por techos.
- La oscuridad del Mutado se prueba con una zona lógica y un oscurecido provisional; la iluminación
  real corresponde a la Fase 7.
- Los enemigos invocados por Chillón y Nidificante se instancian sin pooling; la integración con
  oleadas y pooling corresponde a la Fase 7.
- El arte es provisional, generado a partir del zombi común; el arte final se sustituye sin cambiar
  la lógica.
