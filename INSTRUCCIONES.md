## 1. Visión del juego

- **Género:** acción, disparos y supervivencia en 2D; run-and-gun de desplazamiento horizontal, estilo arcade.
- **Historia:** una infección desconocida, nacida de un experimento fuera de control, convirtió a gran parte de la población en zombis. Cuatro sobrevivientes reciben una señal de auxilio de un antiguo centro de investigación donde podría estar el origen.
- **Victoria:** cruzar cinco zonas, llegar al laboratorio principal y destruir el núcleo de investigación.
- **Público:** aficionados a la acción 2D, los zombis y lo postapocalíptico que buscan controles sencillos y combate directo.
- **Estilo visual:** caricatura detallada con contornos oscuros; escenarios en ruinas y sombríos; verde tóxico para la infección. Interfaz con títulos verde neón y rojo.

## 2. Configuración técnica \[Propuesta\]

- Unity 6.3 LTS, plantilla 2D con URP y luces 2D.
- C#, Input System, Cinemachine, Tilemap y TextMeshPro.
- Plataforma: PC (Windows), teclado y mouse, con soporte para gamepad. Resolución base 1920×1080.
- Un jugador. Sin multijugador en la primera versión.
- Carpetas: `Assets/_Project/` con `Scripts`, `Prefabs`, `Scenes`, `Art`, `Audio`, `Data`, `UI`.
- Escenas: `MainMenu`, `CharacterSelect`, `Level01` a `Level05`, `Victory`.
- Arquitectura: componentes pequeños, interfaz `IDamageable`, máquina de estados para enemigos, eventos para la interfaz y object pooling para balas, enemigos y efectos.
- Git con `.gitignore` de Unity y un commit por fase.

## 3. Controles y mecánicas

Moverse, saltar, esquivar y disparar en todas direcciones. El avance es de izquierda a derecha.

| Acción | Teclado y mouse \[Propuesta\] | Gamepad \[Propuesta\] |
| --- | --- | --- |
| Moverse | A / D | Stick izquierdo |
| Saltar | Espacio | A |
| Esquivar (con invulnerabilidad breve) | Shift | B |
| Apuntar | Mouse (360°) | Stick derecho |
| Disparar | Clic izquierdo | Gatillo derecho |
| Granada o molotov | Clic derecho | Gatillo izquierdo |
| Subir o bajar escaleras | W / S | Stick izquierdo |
| Interactuar | E | X |
| Pausa | Esc | Start |

- Salud: 100. Vidas: 3. Puntos de control a mitad de cada nivel. **\[Propuesta\]**
- La cámara sigue al jugador y respeta los límites del nivel. En las zonas de oleada se bloquea hasta eliminar a los enemigos. **\[Propuesta\]**

## 4. Personajes

El diseño presenta cuatro héroes. **\[Propuesta\]** El jugador elige uno antes de empezar; las estadísticas y habilidades de la tabla son sugeridas.

| Personaje | Rol y aspecto | Salud / velocidad | Habilidad |
| --- | --- | --- | --- |
| Alex | Protagonista; chaleco café, mochila táctica, pistola | 100 / media | Recibe 25 % más munición |
| Sofía | Médico de combate; uniforme verde, cruz roja, botiquín, inyectores | 90 / media-alta | Se cura 25 puntos cada 45 s |
| Marcus | Superviviente pesado; armadura, escudo antidisturbios, escopeta | 140 / baja | Embestida con escudo que bloquea de frente |
| Luna | Exploradora ágil; capucha, mascarilla, ballesta, cuchillos | 75 / alta | Doble salto y cuchillo cuerpo a cuerpo |

Todos pueden recoger cualquier arma. El arma inicial es la pistola; Marcus empieza con escopeta y Luna con ballesta (munición infinita). **\[Propuesta\]**

## 5. Armas y objetos

Valores iniciales **\[Propuesta\]**, para ajustar jugando.

| Arma | Caja | Daño | Disparos/s | Munición | Comportamiento |
| --- | --- | --- | --- | --- | --- |
| Pistola | — | 10 | 4 | Infinita | Semiautomática, equilibrada |
| Escopeta | S | 8 × 6 perdigones | 1.2 | 30 | Gran dispersión, corto alcance |
| Ametralladora pesada | H | 9 | 12 | 200 | Ráfaga rápida, fogonazo intenso |
| Rifle de asalto | AR | 14 | 7 | 120 | Preciso, cadencia alta |
| Lanzacohetes (arma especial) | R | 100 en área | 0.7 | 10 | Misil con estela de humo y explosión |
| Lanzallamas | F | 5 por impacto | 15 | 150 | Chorro corto que quema |
| Granada | — | 80 en área | — | 3 | Arco y explosión con retardo |
| Molotov | — | 10 por s | — | 3 | Deja fuego en el suelo 4 s |

- Al agotar la munición se vuelve al arma inicial.
- **Botiquín:** cura 35 puntos.
- **Power-ups temporales (10 s):** daño ×1.5, velocidad ×1.3, disparo rápido ×1.5.
- Las bonificaciones las sueltan los enemigos especiales.

## 6. Enemigos

Valores **\[Propuesta\]** salvo el comportamiento, tomado de los bocetos.

| Enemigo | Salud | Comportamiento | Niveles |
| --- | --- | --- | --- |
| Común | 30 | Camina hacia el jugador; ataca en grupos pequeños (ciudad) u hordas densas (laboratorio). Punto débil: cabeza, daño ×2 | 1–5 |
| Corredor | 25 | Acecha en sombras con ojos brillantes, corre y salta en emboscada | 2–5 |
| Trepador | 35 | Se adhiere a paredes y techos, usa conductos, embosca y bloquea rutas | 2, 4, 5 |
| Escupidor | 60 | Carga y lanza ácido en arco a larga distancia, cadencia lenta, deja charco dañino | 3–5 |
| Chillón | 40 | Al gritar atrae o genera hordas; queda quieto y vulnerable | 2–5 |
| Blindado | 200 | Lento; el escudo es invulnerable de frente; atacar por la espalda o con explosivos | 3–5 |
| Nidificante | 180 | Lento; regenera zombis cercanos; al morir libera crías rastreras (10 de salud) | 4–5 |
| Mutado | 350 | Quistes verdes que iluminan; movimiento errático; revela al jugador en zonas oscuras (los enemigos cercanos lo detectan) | 5 |

Los tres bocetos a mano (un zombi en carrera, uno con las costillas expuestas y uno barbado con arma de púas) sirven como variantes visuales del corredor, el común y un común con ataque cuerpo a cuerpo.

## 7. Niveles

Cada nivel dura de 4 a 6 minutos y termina con una oleada final. **\[Propuesta\]**

| Nivel | Escenario | Objetivo y cierre | Elementos propios |
| --- | --- | --- | --- |
| 1. Ciudad Abandonada | Calles bloqueadas, autobuses y autos destruidos, ferretería, café | Llegar al punto de evacuación y conseguir un arma mejor (escopeta) | Tutorial de controles |
| 2. Hospital en Ruinas | Recepción, maternidad, laboratorio, camillas | Recuperar información de los primeros casos (interactuar con la computadora) y salir hacia la zona industrial | Zombis rápidos |
| 3. Zona Industrial | Fábricas, almacenes, grúas | Atravesar la fábrica y activar la puerta de seguridad | Enemigos resistentes, cajas colgantes |
| 4. Túneles del Metro | Línea 4, vagones, vías, agua | Cruzar hasta el camino al laboratorio | Poca luz (luces 2D), emboscadas desde varias direcciones |
| 5. Centro de Investigación | Cápsulas, consolas, núcleo verde | Llegar al laboratorio principal y destruir el núcleo | Hordas densas y mutados |

**Final \[Propuesta\]:** el núcleo (1000 de salud) invoca oleadas y solo recibe daño cuando se abre, entre oleada y oleada. Al destruirlo aparece la pantalla de victoria.

## 8. Obstáculos

- **Cajas apiladas** (cruz roja, munición, diana): sirven de plataforma y cobertura; al romperse sueltan botiquín, munición o power-up.
- **Cajas y botiquines colgados de cadenas:** se balancean; al disparar a la cadena caen, dañan enemigos y entregan su contenido.
- **Sacos de arena con cinta policial:** cobertura que obliga a saltar.
- **Andamios con escalera:** plataformas de varios pisos a las que se sube.

## 9. Interfaz y audio

- HUD: salud, vidas, retrato, arma y munición, granadas, power-up activo con temporizador, objetivo actual y puntuación.
- Menús: inicio, selección de personaje, pausa, opciones de volumen, derrota y victoria.
- Textos de historia antes de cada nivel. Todo en español.
- Audio **\[Propuesta\]**: disparos por arma, sonidos de zombis, grito del chillón, explosiones y música por nivel, con sonidos libres o propios.
- Guardado del nivel desbloqueado.

## 10. Fases y criterios de aceptación

| Fase | Entrega | Está lista cuando |
| --- | --- | --- |
| 1. Base | Proyecto, carpetas, Alex, movimiento, salto, esquive, apuntado, pistola, cámara | Se recorre una escena de prueba y se dispara en cualquier dirección |
| 2. Combate | Salud, daño, zombi común, pooling, muerte y reinicio, HUD básico | Se puede ganar y perder contra un grupo |
| 3. Nivel 1 completo | Tilemap, obstáculos, punto de control, escopeta, objetivo | El nivel se juega de principio a fin |
| 4. Arsenal | Todas las armas, secundarias, botiquín, power-ups | Cada objeto se recoge y funciona |
| 5. Enemigos | Los siete restantes y las crías | Cada uno se prueba en una escena de pruebas |
| 6. Personajes | Sofía, Marcus, Luna y selección | Los cuatro completan el nivel 1 |
| 7. Niveles 2–5 | Escenarios, objetivos, núcleo final | La campaña se termina completa |
| 8. Pulido | Menús, audio, efectos, balance, build de Windows | El ejecutable corre sin errores y a 60 FPS |

## 11. Arte

Las láminas del PDF son referencia, no sprites listos para usar. Cada personaje y enemigo necesita imágenes separadas con fondo transparente y animaciones: reposo, correr, saltar, disparar, atacar, recibir daño y morir, según corresponda. Los fondos necesitan capas separadas para el efecto parallax.

## 12. Decisiones por confirmar

- ¿Se elige un héroe, se alterna entre los cuatro o habrá cooperativo?
- ¿Solo PC o también Android? En móvil cambian los controles.
- ¿El lanzacohetes es el “arma especial”? Aquí se asume que sí.
- ¿Habrá jefes además del núcleo final?
- Correcciones al PDF: “Los protagonistas debe” → “deben”; “El Ultimo” → “El Último”; “Héroe” → “Héroes”; “Obstaculos” → “Obstáculos”; la lámina de armas repite “ZONE”.
