# USM Files (CriMovie)

USM is a container format for audio and video, provided by the CRIWARE SDK. Kingdom Hearts III uses CRIWARE mostly for its pre-rendered cutscenes.

## Format
A good in-depth write-up of the USM format can be found [here](https://listed.to/@donmai/24921/criware-s-usm-format-part-1).

With regards to Kingdom Hearts III, USM files contain:
* **Video**: Encrypted video streams, using H264 and MPEG codecs.
* **Audio**: Unencrypted audio streams, using the HCA codec (optional)
* **Alpha**: Alpha masks (optional)

## File List

Some of the USM files are duplicated per language.

| ID  | Language                    |
|-----|-----------------------------|
| en  | English                     |
| ja  | Japanese                    |
| jax | Japanese (for Asian Market) | 

### day1

To prevent pre-release leaks of the epilogue and secret ending, the day1 files were originally shipped in a separate update.

Base Path: ```<Installation Path>\KINGDOM HEARTS III\Content\CriMovie\day1```

| **Folder**    | **File Name**     | **Description**                                                                                                      |
|---------------|-------------------|----------------------------------------------------------------------------------------------------------------------|
| en / ja       | mv_epilogue.usm   | [Epilogue](https://www.youtube.com/watch?v=AhZWb-XEf1M)                                                              |
| en / ja / jax | mv_memory_ep1.usm | [Episode 1: Departures](https://www.youtube.com/watch?v=Qtq3D9kvHt0&list=PLYnY5NdwIhRDYFxexfUfdRQ_WS5a46Stw&index=2) |
| en / ja / jax | mv_memory_ep2.usm | [Episode 2: Memories](https://www.youtube.com/watch?v=9WX2U0DY-aQ&list=PLYnY5NdwIhRDYFxexfUfdRQ_WS5a46Stw&index=2)   |
| en / ja / jax | mv_memory_ep3.usm | [Episode 3: Twilight](https://www.youtube.com/watch?v=CYTcAI5aZng&list=PLYnY5NdwIhRDYFxexfUfdRQ_WS5a46Stw&index=3)   |
| en / ja / jax | mv_memory_ep4.usm | [Episode 4: Dawn](https://www.youtube.com/watch?v=W6iOt2GHBU8&list=PLYnY5NdwIhRDYFxexfUfdRQ_WS5a46Stw&index=4)       |
| en / ja / jax | mv_memory_ep5.usm | [Episode 5: Darkness](https://www.youtube.com/watch?v=ShDbwm_dhII&list=PLYnY5NdwIhRDYFxexfUfdRQ_WS5a46Stw&index=5)   |
| en / ja       | mv_secret.usm     | [Secret Movie](https://www.youtube.com/watch?v=U7YaNjUe01o)                                                          |

### main

Files ending with with ```cXX``` are usually flashback scenes or overlays within a bigger cutscene.

Base Path: ```<Installation Path>\KINGDOM HEARTS III\Content\CriMovie\main```

| **Folder**     | **File Name**             | **Description**                                                                             | **World**                  |
|----------------|---------------------------|---------------------------------------------------------------------------------------------|----------------------------|
| Base Path      | ca406_shot010.usm         | [Air Raid](https://www.youtube.com/watch?v=LXotoQgGnVo)                                     | The Caribbean              |
| Base Path      | ew_timelapse.usm          | [Timelapse of Clouds in Tutorial Section]()                                                 | The Final World            |
| Base Path      | mv_bt051.usm              | [Gambit](https://www.youtube.com/watch?v=6KhyyBBPgtU)                                       | Scala ad Caelum            |
| Base Path      | mv_bx551_c20.usm          | [City of Superheroes (Overlay at 1:07)](https://youtu.be/5M9ixYnG7zY?t=67)                  | San Fransokyo              |
| Base Path      | mv_bx559_c28.usm          | [Enter the Supervillain (Overlay at 1:50)](https://youtu.be/Ihd3Y8yuMZI?t=110)              | San Fransokyo              |
| Base Path      | mv_bx560_c27.usm          | [A Riku From the Past? (1:54)](https://youtu.be/ZcM7Ksd-oto?t=114)                          | San Fransokyo              |
| Base Path      | mv_bx560_c30.usm          | [A Riku From the Past? (2:04)](https://youtu.be/ZcM7Ksd-oto?t=124)                          | San Fransokyo              |
| Base Path      | mv_dw151_c20.usm          | [A Dwindling Trail (2:08)](https://youtu.be/UKYfRekWlNI?t=128)                              | Realm of Darkness          |
| Base Path      | mv_ew755_c12.usm          | [More Than Anyone (0:57)](https://youtu.be/H0PsyeTs2YM?t=57)                                | The Final World            |
| Base Path      | mv_ew755_c14.usm          | [More Than Anyone (1:12)](https://youtu.be/H0PsyeTs2YM?t=73)                                | The Final World            |
| Base Path      | mv_ew755_c16.usm          | [More Than Anyone (1:27)](https://youtu.be/H0PsyeTs2YM?t=87)                                | The Final World            |
| Base Path      | mv_fz462_c11.usm          | [The Sisters' Tale (4:59)](https://youtu.be/eFnXzyhs0fI?t=299)                              | Arendelle                  |
| Base Path      | mv_kg851_c15.usm          | [Light in the Darkness (1:13)](https://youtu.be/mPJciW0Khk0?t=73)                           | Keyblade Graveyard         |
| Base Path      | mv_kg858_c27.usm          | [Riku and Riku (2:03)](https://youtu.be/adx4CljwtRk?t=123)                                  | Keyblade Graveyard         |
| Base Path      | mv_kg872t_c23.usm         | [The Sigil (0:58)](https://youtu.be/cIPLITQ87eY?t=58)                                       | Keyblade Graveyard         |
| Base Path      | mv_kg878_c08.usm          | [Kingdom Hearts (0:36)](https://youtu.be/JqXmfSqnbGo?t=36)                                  | Keyblade Graveyard         |
| Base Path      | mv_kg878_c10.usm          | [Kingdom Hearts (0:43)](https://youtu.be/JqXmfSqnbGo?t=43)                                  | Keyblade Graveyard         |
| Base Path      | mv_kg878_c44.usm          | [Kingdom Hearts (2:11)](https://youtu.be/JqXmfSqnbGo?t=131)                                 | Keyblade Graveyard         |
| Base Path      | mv_kg878_c79b.usm         | [Kingdom Hearts (4:40)](https://youtu.be/JqXmfSqnbGo?t=280)                                 | Keyblade Graveyard         |
| Base Path      | mv_kg977_c56.usm          | [Connecting Hearts (3:07)](https://youtu.be/KZ4XFPSh-wA?t=187)                              | Keyblade Graveyard         |
| Base Path      | mv_rg074_c28.usm          | [The Three Keys (2:19)](https://youtu.be/iuEQ_CyQeOA?t=139)                                 | Radiant Garden             |
| Base Path      | mv_rg232_c12.usm          | [Terra's Whereabouts (0:58)](https://youtu.be/5vW3E6hWtVE?t=58)                             | Radiant Garden             |
| Base Path      | mv_rg232_c19.usm          | [Terra's Whereabouts (2:15)](https://youtu.be/5vW3E6hWtVE?t=135)                            | Radiant Garden             |
| Base Path      | mv_rg232_c27.usm          | [Terra's Whereabouts (03:26)](https://youtu.be/5vW3E6hWtVE?t=206)                           | Radiant Garden             |
| Base Path      | mv_rg292_c10.usm          | [A Replica for Roxas (2:30)](https://youtu.be/0AgeFmoWr34?t=150)                            | Radiant Garden             |
| Base Path      | mv_ts251.usm              | [Commercial Break](https://www.youtube.com/watch?v=C17ho5XMCWg)                             | Toy Box                    |
| Base Path      | mv_tt207_c35.usm          | [Datascapes (5:55)](https://youtu.be/j4w-hg4a8Ag?t=355), probably unused                    | Twilight Town              |
| Base Path      | mv_yt661_c33.usm          | [The Guardians of Light Gather (2:56)](https://youtu.be/QhNGZx0b33I?t=176)                  | Mysterious Tower           |
| Base Path      | select_movie01.usm        | Desire (Vitality)                                                                           | Dive to Heart Choices      |
| Base Path      | select_movie02.usm        | Desire (Wisdom)                                                                             | Dive to Heart Choices      |
| Base Path      | select_movie03.usm        | Desire (Balance)                                                                            | Dive to Heart Choices      |
| Base Path      | select_movie04.usm        | Power (Warrior)                                                                             | Dive to Heart Choices      |
| Base Path      | select_movie05.usm        | Power (Guardian)                                                                            | Dive to Heart Choices      |
| Base Path      | select_movie06.usm        | Power (Mystic)                                                                              | Dive to Heart Choices      |
| Base Path      | select_movie07.usm        | Adventure (Usual)                                                                           | Dive to Heart Choices      |
| Base Path      | select_movie08.usm        | Adventure (Easy)                                                                            | Dive to Heart Choices      |
| Base Path      | select_movie09.usm        | Adventure (Challenging)                                                                     | Dive to Heart Choices      |
| Base Path      | ss_videobillboard_01.usm  | Quadratum Billboard                                                                         | Quadratum                  |
| en / ja        | mv_bt907.usm              | [Checkmate](https://www.youtube.com/watch?v=2LFf5svArRM)                                    | Scala ad Caelum            |
| en / ja        | mv_bx556.usm              | [Making Up the Difference](https://www.youtube.com/watch?v=AjkXKlhCwsw)                     | San Fransokyo              |
| en / ja        | mv_bx561b.usm             | [Friend of Yours?](https://www.youtube.com/watch?v=hbCEP5y-GQE)                             | San Fransokyo              |
| en / ja        | mv_ca407b.usm             | [An Arrangement](https://www.youtube.com/watch?v=2I-lrH1TYWU)                               | The Caribbean              |
| en / ja        | mv_ca420b.usm             | [Time for Your Hearties](https://www.youtube.com/watch?v=oFvKqoKrdP4)                       | The Caribbean              |
| en / ja        | mv_dp072.usm              | [The Secret Promise](https://www.youtube.com/watch?v=m-35pGVD52E)                           | Land of Departure          |
| en / ja        | mv_dp651.usm              | [Castle Oblivion Is Unlocked](https://www.youtube.com/watch?v=iK5rZDQa0Jo)                  | Land of Departure          |
| en / ja        | mv_dw954.usm              | [The Tear](https://www.youtube.com/watch?v=zBGngzd8KI8)                                     | Realm of Darkness          |
| en / ja / jax  | mv_ending.usm             | [Ending](https://www.youtube.com/watch?v=gI8nIyeJfw8)                                       | System / Ending            |
| en / ja        | mv_ew953.usm              | [As the Heart Commands](https://www.youtube.com/watch?v=a7F5tpYaMjA)                        | The Final World            |
| en / ja        | mv_ew959.usm              | [Two More Hearts](https://www.youtube.com/watch?v=4KLd9okEn5s)                              | The Final World            |
| en / ja        | mv_fz456b.usm             | [The Ice Palace](https://www.youtube.com/watch?v=99LlrpFm8mI)                               | Arendelle                  |
| en / ja        | mv_fz461.usm              | [The Sisters' Tale](https://www.youtube.com/watch?v=eFnXzyhs0fI)                            | Arendelle                  |
| en / ja        | mv_fz465b.usm             | [Elsa Pushes Help Away](https://www.youtube.com/watch?v=07DTThx6vDY)                        | Arendelle                  |
| en / ja        | mv_fz471c.usm             | [True Love](https://www.youtube.com/watch?v=nkRUrWXWRVA)                                    | Arendelle                  |
| en / ja        | mv_he113_c14.usm          | [Son of Zeus (1:17)](https://youtu.be/tE-lGaJztUk?t=77)                                     | Olympus                    |
| en / ja        | mv_he118.usm              | [Where He Belongs](https://www.youtube.com/watch?v=9ZxaKGPImIE)                             | Olympus                    |
| en / ja        | mv_he119.usm              | [Seekers of the Black Box](https://www.youtube.com/watch?v=mAcJFtcoxl0)                     | Olympus                    |
| en / ja        | mv_he119r.usm             | [Strategic Moves](https://www.youtube.com/watch?v=YNIVY2uNEoU)                              | Olympus                    |
| en / ja        | mv_kg702r_c1-21.usm       | [Ventus's Heart](https://www.youtube.com/watch?v=3JZ3PwOeVJk)                               | Keyblade Graveyard         |
| en / ja        | mv_kg702s_c1-21.usm       | [Aqua's Heart](https://www.youtube.com/watch?v=PjOGsTty0Vk)                                 | Keyblade Graveyard         |
| en / ja        | mv_kg852r.usm             | [Naminé Calls Out](https://www.youtube.com/watch?v=huNqPcVrEk0)                             | Keyblade Graveyard         |
| en / ja        | mv_kg870r_c1-14.usm       | [Terra's Heart (3:31)](https://youtu.be/v24y_G2OyYU?t=211)                                  | Keyblade Graveyard         |
| en / ja        | mv_kg974_bt905.usm        | [Battle of the Guardians (1:16)](https://youtu.be/giLMKs_IxJQ?t=76)                         | Scala ad Caelum            |
| en / ja        | mv_kg977_bt_04_ms060m.usm | [Connecting Hearts](https://www.youtube.com/watch?v=KZ4XFPSh-wA)                            | Keyblade Graveyard         |
| en / ja / jax  | mv_loopdemo.usm           | [Title Screen Loop](https://www.youtube.com/watch?v=PYAlnhy88Y4)                            | N/A                        |
| en / ja / jax  | mv_opening.usm            | [Opening](https://www.youtube.com/watch?v=t9bsdJ3jtnE)                                      | N/A                        |
| en / ja        | mv_ra301.usm              | [A World Outside Her Window](https://www.youtube.com/watch?v=AMlja7lljj4)                   | Kingdom of Corona          |
| en / ja        | mv_ra305.usm              | [Destined to Meet](https://www.youtube.com/watch?v=hxpXhpf3ByI)                             | Kingdom of Corona          |
| en / ja        | mv_ra309.usm              | [A Precious Gift](https://www.youtube.com/watch?v=ADhF0XVXBms)                              | Kingdom of Corona          |
| en / ja        | mv_ra318.usm              | [Rapunzel Remembers](https://www.youtube.com/watch?v=94FsStSCB54)                           | Kingdom of Corona          |
| en / ja        | mv_ra321.usm              | [Flynn Is Attacked](https://www.youtube.com/watch?v=VfgEns_C1_g)                            | Kingdom of Corona          |
| en / ja        | mv_ra322.usm              | [One Power Fades](https://www.youtube.com/watch?v=-a1lwbtA2WA)                              | Kingdom of Corona          |
| en / ja        | mv_ra324.usm              | [A Stronger Power Rises](https://www.youtube.com/watch?v=wmcfDJKece4)                       | Kingdom of Corona          |
| en / ja        | mv_ra326.usm              | [It Is Etched](https://www.youtube.com/watch?v=SmrDDz8pZAk)                                 | Kingdom of Corona          |
| en / ja        | mv_sf231.usm              | [Talking On Paper](https://www.youtube.com/watch?v=BX_TT3XhC5E)                             | San Fransokyo              |
| en / ja        | mv_sf481.usm              | [Nothing's As It Should Be](https://www.youtube.com/watch?v=CqoYQGTz5Cc)                    | San Fransokyo              |
| en / ja        | mv_ss921r.usm             | [Not Over Yet](https://www.youtube.com/watch?v=KCJeVq_NFxA)                                 | Quadratum / Secret Episode |
| en / ja / jax  | mv_ss921s.usm             | [Another Ending](https://www.youtube.com/watch?v=WpiAo2_H5WQ)                               | Multiple                   |
| en / ja        | mv_ss941r.usm             | [Secret Ending (ReMind)](https://www.youtube.com/watch?v=U7YaNjUe01o)                       | Quadratum / Secret Episode |
| en / ja        | mv_ss942_c1-56.usm        | [Sora and Yozora](https://www.youtube.com/watch?v=1MfF31X23IY)                              | Quadratum / Secret Episode |
| en / ja        | mv_ss943.usm              | [I've Been Having These Weird Thoughts Lately](https://www.youtube.com/watch?v=F95m77pootA) | Quadratum / Secret Episode |
| en / ja / jax  | mv_ss945.usm              | [Like, Is Any of This for Real or Not?](https://www.youtube.com/watch?v=O3yomrF_gs0)        | Quadratum / Secret Episode |
| livefeed       | bx_livefeed_0100.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0110.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0120.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0130.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0140.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0150.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0160.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0200.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0210.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0220.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0230.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0240.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0250.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| livefeed       | bx_livefeed_0260.usm      | Transmission from Hiro                                                                      | San Fransokyo              |
| OutdoorTheater | Building_a_Building.usm   | Twilight Town Theater                                                                       | Twilight Town              |
| OutdoorTheater | Giantland.usm             | Twilight Town Theater                                                                       | Twilight Town              |
| OutdoorTheater | Musical_Farmer.usm        | Twilight Town Theater                                                                       | Twilight Town              |
| OutdoorTheater | The_Mad_Doctor.usm        | Twilight Town Theater                                                                       | Twilight Town              |
| OutdoorTheater | The_Mail_Pilot.usm        | Twilight Town Theater                                                                       | Twilight Town              |
| WorldTitle     | 100AcreWood.usm           | World Title Animation                                                                       | 100 Acre Wood              |
| WorldTitle     | Arendelle.usm             | World Title Animation                                                                       | Arendelle                  |
| WorldTitle     | Darkworld.usm             | World Title Animation                                                                       | Realm of Darkness          |
| WorldTitle     | KeybladeGraveyard.usm     | World Title Animation                                                                       | Keyblade Graveyard         |
| WorldTitle     | Monstropolis.usm          | World Title Animation                                                                       | Monstropolis               |
| WorldTitle     | MysteriousTower.usm       | World Title Animation                                                                       | Mysterious Tower           |
| WorldTitle     | Olympos.usm               | World Title Animation                                                                       | Olympus                    |
| WorldTitle     | SanFransokyo.usm          | World Title Animation                                                                       | San Fransokyo              |
| WorldTitle     | ScalaAdCaelum.usm         | World Title Animation                                                                       | Scala ad Caelum            |
| WorldTitle     | TheCaribbean.usm          | World Title Animation                                                                       | The Caribbean              |
| WorldTitle     | TheFinalWorld.usm         | World Title Animation                                                                       | The Final World            |
| WorldTitle     | ToyBox.usm                | World Title Animation                                                                       | Toy Box                    |
| WorldTitle     | TwilightTown.usm          | World Title Animation                                                                       | Twilight Town              |