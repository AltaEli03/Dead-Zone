# Fase 7: Campaña 2–5 — Diseño

## Objetivo

Convertir el Nivel 1 en el arranque de una campaña completa: Niveles 2–5, objetivos propios, núcleo
final y pantalla de victoria, conservando el personaje elegido.

## Enfoque

- Cada nivel parte de la estructura de `Level01` y se diferencia por iluminación, tinte del suelo,
  etiquetas de escenario, enemigos, armas y objetivo.
- `LevelController` pasa a ser genérico: objetivo, oleada final opcional, núcleo opcional y
  siguiente escena.
- El personaje se guarda en `PlayerSelection`; solo se elige en el Nivel 1 y se aplica en los demás.
- `ObjectivePoint` resuelve la computadora del hospital y la puerta de seguridad.
- `CoreBoss` implementa el núcleo final: 1000 de vida, oleadas y ventanas de vulnerabilidad.
- `VictoryScreen` cierra la campaña y devuelve al Nivel 1.

## Niveles

| Nivel | Escenario | Objetivo | Enemigos propios | Armamento |
| --- | --- | --- | --- | --- |
| 2 | Hospital en Ruinas | Recuperar la información y salir | Corredores | AR, HMG, lanzacohetes |
| 3 | Zona Industrial | Activar la puerta de seguridad y salir | Blindado, Escupidor | HMG, lanzacohetes, AR |
| 4 | Túneles del Metro | Cruzar hasta la salida | Trepador, Chillón; luz 0,45 | Lanzallamas, AR |
| 5 | Centro de Investigación | Destruir el núcleo | Nidificante, Mutado; zona oscura | HMG, lanzacohetes, AR, lanzallamas |

Cada nivel conserva checkpoint, oleada final (salvo el 5, que la sustituye por las oleadas del
núcleo), salida y HUD. El núcleo se abre al limpiar cada oleada, recibe daño solo abierto y al morir
completa la campaña.

## Flujo

`Level01 → Level02 → Level03 → Level04 → Level05 → Victory → Level01`

Las escenas están en Build Settings. La selección se muestra solo si no hay personaje guardado.

## Verificación

- Campos de nivel y siguiente escena en los cinco niveles.
- Transiciones Level01→02→03→04→05→Victory.
- Objetivos de computadora y puerta, oleadas finales y salidas.
- Núcleo: 1000/1000 protegido, apertura tras la oleada, daño solo abierto, muerte y victoria.
- Persistencia del personaje elegido entre niveles y reinicio de la selección en Victory.
- Consola limpia y escenas guardadas.

## Pendiente / límites

- Los escenarios reutilizan la planta del Nivel 1 con tema, etiquetas y props; el arte y layouts
  definitivos son de la Fase 8.
- Falta la pasada manual completa de la campaña con los cuatro personajes.
- El balance del núcleo y de las oleadas necesita ajuste tras jugar.
- Menú principal, selección dedicada y audio quedan para la Fase 8.
