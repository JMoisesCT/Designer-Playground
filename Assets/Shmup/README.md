# Tutorial: tu primer juego de naves

En este tutorial vas a crear un nivel de un juego de disparos con naves (*shoot 'em up*), **sin escribir código**: dibujas las islas, colocas oleadas de enemigos, torretas y mejoras, eliges al jefe final, ajustas tu nave y lo pruebas.

Tiempo: unos 55 minutos.

---

## 0. Antes de empezar: la ventana de Unity

Abre el proyecto desde **Unity Hub**. Estas son las partes de la ventana que vas a usar:

| Panel | Para qué sirve |
|---|---|
| **Scene** (centro) | Donde construyes el nivel. Rueda del ratón = zoom; botón central (o `Alt` + clic derecho) = moverte. |
| **Game** (pestaña junto a Scene) | Lo que ve el jugador al darle a Play. |
| **Hierarchy** (izquierda) | La lista de objetos del nivel: Jugador, cámara, enemigos... |
| **Inspector** (derecha) | Los valores del objeto que tengas seleccionado. Aquí se ajusta todo. |
| **Project** (abajo) | Los archivos del proyecto. Lo del juego de naves está en `Assets/Shmup/`. |

> **Truco:** con el ratón sobre la vista Scene, pulsa `F` para centrar la vista en el objeto seleccionado.

---

## 1. Mira el nivel de ejemplo

1. En **Project**, abre `Assets/Shmup/Scenes/` y haz doble clic en **Nivel_01**.
2. Pulsa **Play** (el triángulo arriba, en el centro) y juega:

| Acción | Teclado | Mando |
|---|---|---|
| Mover la nave | `W` `A` `S` `D` o flechas | Stick izquierdo o cruceta |
| Disparar (mantén pulsado) | `Espacio` | Botón inferior (A / Cruz) |

3. La pantalla **avanza sola** hacia la derecha: tú solo te mueves dentro de ella.
4. Cada enemigo destruido da **puntos** (arriba a la derecha). Si te dan, pierdes una **vida** (arriba a la izquierda) y parpadeas un momento: mientras parpadeas no pierdes más.
5. Recoge la **P**: mejora tu arma (doble y luego triple). El **botiquín** te devuelve una vida.
6. Cuidado con la **avioneta amarilla** que se queda en pantalla: es un **Potenciador** y los enemigos cercanos se ponen morados (disparan el doble). Después, una nave verde lanza **granadas** que estallan en círculo.
7. Al final, la pantalla se para y aparece el **jefe**. Destrúyelo para completar el nivel.
8. Pulsa **Play** otra vez para salir.

> ⚠️ **Muy importante:** lo que cambies **mientras está en Play** se pierde al salir. Sal siempre de Play antes de editar.

No edites `Nivel_01`: déjalo como ejemplo. Vas a crear el tuyo.

---

## 2. Crea tu nivel

1. Menú superior **Shmup → Crear nivel nuevo**.
2. Ponle tu nombre, por ejemplo `Nivel_Ana`, y pulsa **Guardar**.

Tu nivel ya trae dos islas, la nave y la meta. Puedes darle a Play ya mismo:

| En la Hierarchy | Qué es |
|---|---|
| `Grid → Islas` | Las islas. El agua no se pinta: es el color de fondo. |
| `Grid → Decorado` | Lo que va encima: árboles, casas e islotes. |
| `Jugador` | Tu nave. |
| `Meta` | El final del nivel: ahí se para la pantalla. |
| `Main Camera` | La cámara. Avanza sola. Dentro lleva la **Musica**. |
| `Interfaz` | Las vidas, los puntos y el mensaje de "¡Nivel completado!". |
| `Global Light 2D` | La luz. **No la borres** o todo se verá negro. |

**El rectángulo verde** de la vista Scene es la zona que recorre la pantalla, desde tu nave hasta la Meta. **Construye tu nivel dentro de él.** Si no lo ves, selecciona `Main Camera` y comprueba que el botón **Gizmos** de la vista Scene está activado.

---

## 3. Dibuja el escenario

Al crear el nivel se abrió la ventana **Tile Palette** (si la cerraste: *Window → 2D → Tile Palette*). Arriba, comprueba que la paleta es **Shmup**.

El nivel tiene **dos capas** que se pintan por separado. Antes de pintar, elige la capa en el desplegable **Active Tilemap** de la Tile Palette:

| Fila de la paleta | Qué es | Active Tilemap |
|---|---|---|
| Arriba: **Isla** (verde) y **Desierto** (marrón) | El terreno | **Islas** |
| Segunda fila: arbustos, árboles, casas, bandera, huerto | Adornos para la isla verde | **Decorado** |
| Tercera fila: lo mismo en versión desierto | Adornos para el desierto | **Decorado** |
| Abajo: islotes | Adornos para el agua | **Decorado** |

1. Haz clic en un tile de la paleta.
2. Pinta en la vista **Scene** con clic izquierdo.

**Las islas** ponen su orilla **solas**: pinta de cualquier forma y la orilla se ajusta. Las islas se ven mejor si miden al menos 3 tiles de ancho y de alto.

**Los adornos** llevan su trozo de suelo pegado: píntalos **dentro** de la isla, no en la orilla, y cada uno en su tipo de isla.

**Herramientas** (con el ratón sobre la vista Scene):

| Tecla | Herramienta |
|---|---|
| `B` | Pincel: pinta tile a tile |
| `U` | Rectángulo: arrastra para rellenar un área de golpe |
| `D` | Borrador (o mantén `Shift` mientras pintas) |
| `I` | Cuentagotas: copia un tile que ya está en la escena |
| `Ctrl + Z` | Deshacer |

> **Medidas:** 1 tile = 1 cuadrito de la grilla. La pantalla muestra 24 × 13 tiles y la nave mide 2 tiles. El terreno ayuda a notar que avanzas: si hay mucha agua vacía, parece que estás quieto. Pon islotes.

**✏️ Prueba:** pinta una isla verde con un pueblo y un desierto con árboles. Dale a Play y mira cómo pasan.

---

## 4. Enemigos

Los objetos listos para usar están en `Assets/Shmup/Prefabs/`. **Arrástralos** desde Project hasta la vista Scene, dentro del rectángulo verde.

Un enemigo **espera quieto** hasta que entra en pantalla. Así, cuanto más a la derecha lo pongas, más tarde aparece: la posición es el momento en que sale.

| Prefab | Qué hace | Balazos que aguanta |
|---|---|---|
| **EnemigoRecto** | Cruza la pantalla en línea recta. | 1 |
| **EnemigoOndulante** | Avanza ondulando arriba y abajo. | 2 |
| **EnemigoTirador** | Nave lenta que te dispara. | 5 |
| **Torreta** | Cañón fijo: no se mueve, pero te dispara. Ponla en una isla. | 4 |
| **Jefe** | Nave enorme que se queda en pantalla hasta que la destruyas. | 100 |
| **EnemigoGranadero** | Nave verde lenta que lanza **granadas**: vuelan despacio y, al rato, **estallan** en 8 balas en círculo. | 6 |
| **Potenciador** | Avioneta amarilla de apoyo que se queda en pantalla. Mientras viva, los enemigos de su alrededor se ponen **morados**: disparan el doble de rápido y vuelan más deprisa. ¡Destrúyela primero! | 6 |

**Oleadas:** coloca varios enemigos iguales en fila, en columna o en V. Con `Ctrl + D` duplicas el seleccionado, y con `Ctrl` mientras lo arrastras se mueve tile a tile.

### Valores de los enemigos

Selecciona un enemigo. Al seleccionarlo, una **línea amarilla** enseña el camino que seguirá.

| Componente | Valor | Qué cambia |
|---|---|---|
| *Enemy* | **Health** | Balazos que aguanta. |
| *Enemy* | **Points** | Puntos que da. |
| *Enemy* | **Drop** | Lo que suelta al destruirlo: arrastra aquí un prefab, como **Mejora**. |
| *Enemy Mover* | **Speed** | Velocidad, en tiles por segundo. |
| *Enemy Mover* | **Wave Height** | Cuánto ondula, en tiles. 0 = recto. |
| *Enemy Mover* | **Wave Time** | Segundos que tarda una onda completa. |
| *Enemy Shooter* | **Fire Interval** | Segundos entre disparos. |
| *Enemy Shooter* | **Bullet Count** | Balas por disparo. |
| *Enemy Shooter* | **Spread Angle** | Ángulo del abanico si hay varias balas. Con 360 salen en círculo. |
| *Enemy Shooter* | **Aim At Player** | Activado: apunta a tu nave. Desactivado: dispara hacia donde mira. |

**Cambiar la dirección:** el enemigo vuela hacia donde apunta su morro. Gíralo con la herramienta de rotar (tecla `E`) o cambia **Rotation Z** en el Inspector: 90 = hacia la izquierda, 135 = en diagonal hacia abajo, 45 = en diagonal hacia arriba.

> Si un enemigo entra por arriba o por abajo, ponlo justo fuera de la pantalla: a más de 2 tiles del borde no llega a despertarse.

**Que dispare cualquiera:** con un EnemigoRecto seleccionado, pulsa **Add Component**, elige *Enemy Shooter* y arrastra a **Bullet Prefab** la **BalaEnemiga** de `Assets/Shmup/Prefabs/`.

### Granadas

La **Granada** (en Prefabs) es una bala que estalla. Selecciónala para ver en **rojo** las direcciones de sus fragmentos:

| Valor (*Grenade*) | Qué cambia |
|---|---|
| **Fuse Time** | Segundos que vuela hasta estallar |
| **Fragment Count** | Balas que salen al estallar, repartidas en círculo |
| **Fragment Prefab** | La bala de cada fragmento (por defecto, **BalaEnemiga**) |

La velocidad de la granada es **Speed** en su *Bullet*. Para que cualquier enemigo lance granadas, arrastra la **Granada** a su **Bullet Prefab** (en *Enemy Shooter*).

> **Granadas para tu nave:** duplica la Granada (`Ctrl + D` en Project), activa **From Player** en su *Bullet*, pon **BalaJugador** en *Fragment Prefab*, cambia su *Layer* (arriba del Inspector) a **Player** y arrástrala a **Bullet Prefab** del Jugador.

### El Potenciador

Al seleccionarlo verás un **círculo morado**: los enemigos que estén dentro (y ya en pantalla) quedan potenciados. Si lo destruyes, el efecto se acaba al instante.

| Valor (*Enemy Booster*) | Qué cambia |
|---|---|
| **Radius** | Tamaño del círculo morado, en tiles |
| **Fire Rate** | Cuánto más rápido disparan: 2 = el doble |
| **Speed** | Cuánto más rápido vuelan: 1,5 = un 50 % más |

Como tiene *Stay On Screen* activado, te acompaña hasta que lo derribes, ¡también contra el jefe!

**✏️ Prueba:** haz una V de cinco EnemigoRecto y, detrás, una fila de EnemigoOndulante con *Wave Height* distintos. Después pon un Potenciador detrás de una Torreta: ¿cuál destruyes primero?

---

## 5. Mejoras y botiquines

| Prefab | Qué hace |
|---|---|
| **Mejora** | Sube tu arma un nivel: de una bala a doble y de doble a triple. |
| **Botiquin** | Recupera una vida. |

Se quedan quietos en el mapa: la pantalla los trae hacia ti. Colócalos a mano o arrástralos al campo **Drop** de un enemigo para premiar al que lo destruya.

**✏️ Prueba:** pon una Mejora en una esquina difícil, rodeada de torretas. ¿Merece la pena el riesgo?

---

## 6. El final del nivel

La pantalla se para al llegar a la **Meta**. Cuando ya no queda ningún enemigo en pantalla, el nivel se completa.

- **Mueve la Meta** para hacer el nivel más largo o más corto (el rectángulo verde se estira con ella). A 2 tiles por segundo, cada 10 tiles de más son 5 segundos de juego.
- **Con jefe:** arrastra un **Jefe** unos tiles a la izquierda de la Meta. Sin jefe, el nivel termina al llegar a ella.
- Todo lo que esté **a la derecha de la Meta** no aparecerá nunca.

Selecciona **Meta** en la Hierarchy:

| Valor | Qué cambia |
|---|---|
| **Next Level** | Arrastra aquí la escena del siguiente nivel (desde `Assets/Shmup/Scenes/`). Si lo dejas vacío, el nivel vuelve a empezar. |
| **Message** | El texto que sale al terminar. |
| **Delay** | Segundos que se ve el mensaje antes de cambiar de nivel. |

Para ajustar al jefe, además de los valores del apartado 4:

| Valor (*Enemy Mover*) | Qué cambia |
|---|---|
| **Stay On Screen** | Activado: se queda en pantalla hasta que lo destruyan. Puedes activarlo en cualquier enemigo. |
| **Screen Distance** | A cuántos tiles del borde derecho se queda. |

> **Truco para probar:** la pantalla empieza donde esté tu nave. Arrastra el **Jugador** cerca del final y dale a Play para probar al jefe sin jugar todo el nivel. Después, devuélvelo al principio.

**✏️ Prueba:** crea un segundo nivel y arrástralo a *Next Level* de la Meta del primero. ¡Ya tienes un juego con dos niveles!

---

## 7. Ajusta tu nave

Selecciona **Jugador** en la Hierarchy. En el Inspector:

| Componente | Valor | Qué cambia |
|---|---|---|
| *Player Controller* | **Move Speed** | Velocidad de la nave, en tiles por segundo |
| *Player Controller* | **Fire Rate** | Disparos por segundo |
| *Player Controller* | **Weapon Level** | Arma inicial: 1 = una bala, 2 = doble, 3 = triple |
| *Player Health* | **Max Health** | Vidas con las que empieza (de 1 a 10) |
| *Player Health* | **Invulnerable Time** | Segundos sin recibir daño después de un golpe |
| *Player Health* | **Restart Delay** | Segundos desde que pierde la última vida hasta que el nivel vuelve a empezar |

La velocidad y el daño de las balas se cambian en los prefabs **BalaJugador** y **BalaEnemiga** (*Bullet → Speed* y *Damage*). Afecta a todos los niveles del proyecto: si trabajas con compañeros, avísales.

Para cambiar el ritmo del nivel, selecciona **Main Camera** y ajusta **Speed** en *Scroll Camera* (tiles por segundo).

**✏️ Prueba:** pon *Speed* de la cámara en 5. ¿Te da tiempo a esquivar? ¿Y con *Move Speed* en 4?

---

## 8. Cambia la música y los sonidos

**Música:** en la Hierarchy, abre **Main Camera** y selecciona **Musica**. En el Inspector, arrastra otra canción al campo **Audio Resource**. Hay varias en `Assets/ThirdParty/NinjaAdventure/Audio/Musics/`. Con **Volume** la subes o bajas.

**Sonidos:** cada objeto trae los suyos: disparo (*Shot Sound*), golpe y explosión (*Hit Sound*, *Hurt Sound* y *Death Sound*), objetos y Meta (**Sound**). Arrastra otro desde `Assets/ThirdParty/Kenney/Audio_DigitalSFX/Audio/` o déjalo vacío para que no suene.

> **Truco:** haz clic en un sonido en la ventana Project y pulsa el botón ▶ de la parte de abajo del Inspector para escucharlo.

---

## 9. Guarda

`Ctrl + S`. Guarda a menudo.

---

## Retos

1. **La escuadrilla:** dibuja una flecha con EnemigoRecto. ¿Se puede pasar sin disparar?
2. **El muro:** una columna de enemigos de arriba abajo con un solo hueco. Muévelo en cada oleada.
3. **Por todas partes:** enemigos que entran por arriba y por abajo, girados en diagonal.
4. **Cañones en la costa:** una isla llena de torretas con una Mejora en el centro.
5. **Tormenta de balas:** un EnemigoTirador con *Bullet Count* 9, *Spread Angle* 360 y *Aim At Player* desactivado.
6. **Tu propio jefe:** cambia los valores del Jefe para que tenga su propio estilo. Prueba *Fire Interval* 0.5 con *Bullet Count* 3.
7. **Dos jefes:** ponlos con *Screen Distance* distintos para que no se pisen.
8. **Una sola vida:** pon *Max Health* en 1 y diseña un nivel difícil pero justo. Los botiquines ya no sirven: ¿qué premios pondrías?
9. **La saga:** tres niveles encadenados con *Next Level*, cada uno más rápido que el anterior.
10. **Escolta:** un Jefe con dos Potenciadores. ¿Se puede ganar sin derribarlos antes?
11. **Lluvia de metralla:** tres EnemigoGranadero con *Fragment Count* distintos. Prueba una granada que tarde más (*Fuse Time*).
12. **Intercambio:** juega el nivel de un compañero y dile qué oleada fue la más divertida y dónde te pareció injusto.

---

## Si algo no funciona

| Pasa esto | Solución |
|---|---|
| Todo se ve negro | Borraste `Global Light 2D`. Crea un nivel nuevo, o copia la luz de otro nivel. |
| No pinta donde hago clic | Comprueba el **Active Tilemap** de la Tile Palette y que estás en la vista **Scene** (no en Game). |
| La orilla de una isla se rompe | Hay un adorno pintado en **Islas**. Bórralo y píntalo en **Decorado**. |
| Un adorno tiene un cuadrado de otro color alrededor | Lo pintaste en la orilla o en la isla que no es: píntalo dentro de su tipo de isla. |
| No encuentro la paleta | *Window → 2D → Tile Palette* y elige **Shmup** en el desplegable. |
| La pantalla no avanza | *Speed* de *Scroll Camera* está en 0, o la Meta está demasiado cerca de la nave. |
| La pantalla no se para nunca | Falta la Meta: arrastra el prefab **Meta** al final del nivel. |
| Un enemigo no aparece | Está a la derecha de la Meta, o muy lejos por encima o por debajo de la pantalla. Ponlo dentro del rectángulo verde. |
| Se paró la pantalla y el nivel no acaba | Queda un enemigo en pantalla, quizá una Torreta o uno con *Stay On Screen*: destrúyelo. |
| Un enemigo vuela hacia atrás | Tiene **Rotation Z** cambiada: ponla en 90 para que vaya a la izquierda. |
| La línea amarilla no se ve | Selecciona el enemigo y comprueba que el botón **Gizmos** de la vista Scene está activado. Si *Speed* es 0, no hay línea. |
| La nave no dispara | Mantén pulsado `Espacio`. Haz clic en la vista Game si el juego no responde. |
| No veo las vidas ni los puntos | Arrastra el prefab **Interfaz** a la escena (solo uno). |
| La Meta no lleva al siguiente nivel | Arrastra la escena a *Next Level*. Para el juego exportado, añade todos los niveles en *File → Build Profiles → Scene List*. |
| No suena nada | Comprueba que el botón **Mute Audio** (el altavoz de la barra de la vista Game) no está activado. |
| Los enemigos se ven morados y disparan muy rápido | Hay un **Potenciador** cerca: destrúyelo y vuelven a la normalidad. |
| La granada no estalla | Le das antes con la nave (choca contigo) o sale de la pantalla antes de *Fuse Time*: bájalo. |
| Cambié algo y se perdió | Lo cambiaste en Play. Sal de Play y vuelve a cambiarlo. |
