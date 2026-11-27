# Sprites provisionales

Todos los sprites son originales, están hechos con formas y colores y se importan a 100 PPU.
Cuando llegue el arte final, sustituye el sprite de cada `SpriteRenderer` sin cambiar la lógica.

| Archivo | Dónde se usa | Pivote / notas |
| --- | --- | --- |
| `Alex.png` | `Assets/_Project/Prefabs/Player/Alex.prefab > Visual` | Pivote abajo-centro (0.5, 0). Los pies deben quedar en el borde inferior. El cuerpo no incluye el brazo que apunta. |
| `AlexCrouch.png` | `Alex.prefab > Visual` cuando está agachado | Pivote abajo-centro (0.5, 0). El hombro del brazo+arma baja a 0.52. |
| `ArmPistol.png` | `Alex.prefab > WeaponPivot > Arm` | Pivote izquierda-centro (0, 0.5), en el hombro. El brazo y el arma son una sola pieza: la pistola queda siempre en la mano. El cañón apunta a +X y el muzzle está en 0.70. |
| `Bullet.png` | `Assets/_Project/Prefabs/Weapons/Bullet.prefab > SpriteRenderer` | La punta apunta a +X. El collider es un círculo de radio 0.06. |
| `Platform.png` | `Test_Phase01 > Level > Platform_*` | El sprite mide 1.28 x 0.48 unidades. Si cambias su proporción, ajusta el `BoxCollider2D` y el `localScale` de cada plataforma. |

Para que el pivote personalizado se aplique en Unity, el importador debe usar
`Sprite Alignment: Custom`. Los tres sprites de Alex usan abajo-centro; el de brazo+arma,
izquierda-centro.

## Controles (estilo Metal Slug, sin ratón)

| Acción | Teclado | Gamepad |
| --- | --- | --- |
| Mover y apuntar en 8 direcciones | WASD | Stick izquierdo |
| Agacharse (en el suelo) | S | Stick abajo |
| Saltar | Espacio | A |
| Slide (esquiva invulnerable) | Agachado + Espacio | Agachado + A |
| Disparar | J | Gatillo derecho |
| Secundaria / cambiar | K / Q | LT / LB |

El vector de movimiento es también el vector de apuntado. Al soltar la dirección, el arma vuelve
a la horizontal en el último lado al que mira el personaje, su estado normal. W apunta arriba en
suelo o aire; S apunta abajo en el aire y agacha en el suelo.

Agachado: el personaje no avanza, reduce su collider y dispara en horizontal. A/D o el eje
horizontal del stick cambian el lado al que mira sin levantarse. W lo levanta. Para saltar desde
agachado, suelta S y pulsa salto.

Slide: agachado + salto ejecuta el esquive deslizante. Mantiene la postura y el collider
agachados, fija la dirección al empezar, avanza con la velocidad y duración del esquive y usa su
mismo enfriamiento. Durante el slide es invulnerable y puede atravesar a los enemigos. Al terminar,
si sigues manteniendo S, continúa agachado; si lo sueltas, se levanta. El arte provisional usa la
pose de agachado; el arte final puede añadir una pose de slide.

## Apuntado y agachado

El arma gira en `WeaponPivot` en incrementos de 45° sobre el eje Z. El cuerpo solo se voltea en
horizontal. Para el arte final hacen falta como máximo ocho poses de brazo+arma (cinco únicas y
sus espejadas), más una pose de cuerpo agachado.

El brazo+arma se voltea en vertical (`flipY`) cuando el personaje mira a la izquierda para que la
pistola no quede boca abajo.

El collider agachado se configura en `AlexConfig.asset`:
`crouchColliderHeight` (0.9) y `crouchColliderOffsetY` (0.45). El de pie se lee del prefab.
