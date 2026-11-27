# Fase 5: Enemigos — Plan de implementación

> Documento de ejecución de la fase. No se ha hecho commit; el usuario no lo ha pedido.

**Objetivo:** Añadir los siete enemigos restantes y las crías, con una escena de pruebas.

**Arquitectura:** `EnemyAgent` base, `ZombieChase` para persecución, una clase por comportamiento,
datos en `EnemyData` y una escena `Test_Phase05`.

**Spec:** `docs/superpowers/specs/2026-11-26-fase-5-enemigos-design.md`

## Tareas

- [x] **Daño direccional y salud**
  - Crear `Core/DamageInfo.cs`.
  - Añadir `TakeDamage(DamageInfo)` a `IDamageable`, `PlayerHealth` y `EnemyHealth`.
  - Actualizar proyectiles, explosiones y zonas de fuego.
  - Añadir `EnemyHealth.Heal` y el evento `Died`.

- [x] **Base de enemigos**
  - Crear `EnemyAgent` con objetivo, caída, ataque y helpers de movimiento.
  - Convertir `ZombieChase` en subclase.
  - Ampliar `EnemyData` con `EnemyKind`, rango de activación y campos por comportamiento.

- [x] **Comportamientos**
  - `RunnerEnemy`: emboscada, persecución rápida y salto.
  - `ClimberEnemy`: pared, trepa y caída.
  - `SpitterEnemy`, `EnemyAcidProjectile` y `AcidPool`.
  - `ScreamerEnemy`: grito e invocación.
  - `ArmoredEnemy` y `Shield`: bloqueo frontal direccional.
  - `NesterEnemy`: regeneración y crías al morir.
  - `MutantEnemy`: movimiento errático y revelación.
  - `PlayerVisibility` y `DarknessZone`.

- [x] **Datos y arte provisional**
  - Ocho assets de datos de enemigo.
  - Variantes de sprite del zombi común, escudo y ácido.
  - Prefabs `ZombieRunner`, `ZombieClimber`, `ZombieSpitter`, `ZombieScreamer`, `ZombieArmored`,
    `ZombieNester`, `ZombieMutant`, `ZombieOffspring`.
  - Prefabs `AcidProjectile` y `AcidPool`.

- [x] **Escena de pruebas**
  - Copiar `Test_Phase01` a `Test_Phase05`.
  - Desactivar el spawner inicial.
  - Colocar enemigos, pared del Trepador, zona oscura y etiquetas.
  - Añadir la escena a Build Settings.

- [x] **Verificación**
  - Compilación sin errores y consola limpia.
  - Escudo frontal/espalda/explosivo.
  - Regeneración y crías del Nidificante.
  - Invocación del Chillón.
  - Pared y caída del Trepador.
  - Daño del charco de ácido.
  - Ocultación y revelación en la zona oscura.
  - Despertar y ataque del Corredor.
  - Escena sin cambios pendientes.

## Cómo probar

1. Abrir `Assets/_Project/Scenes/Test_Phase05.unity` y pulsar Play.
2. Ir a la izquierda para el Trepador: está colgado de la pared; al acercarse cae.
3. Avanzar por las plataformas: Corredor, Blindado, Nidificante y Chillón.
4. Usar el lanzacohetes o rodear al Blindado para comprobar el escudo.
5. Dejar que el Chillón grite y ver aparecer los comunes; matar al Nidificante para las crías.
6. Subir a la plataforma alta: Escupidor, Mutado, común y zona oscura.
7. Entrar en la zona oscura: el HUD/estado lógico oculta al jugador; el Mutado cercano lo revela.
