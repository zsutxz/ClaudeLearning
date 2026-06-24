#!/usr/bin/env python3
"""
Top scorer player data for 2026 World Cup prediction.
Each team has a list of (player_name, goal_share) tuples.
goal_share = proportion of team's goals this player is expected to score.
Includes a "other" category for balance.
"""
TEAM_SCORERS = {
    "Argentina": [
        ("Lionel Messi", 0.28),        # Penalty taker, captain, still elite playmaker
        ("Lautaro Martínez", 0.24),      # Serie A top scorer, main striker
        ("Julián Álvarez", 0.16),        # Man City, strong form
        ("Ángel Di María", 0.10),        # Big game player, aging
        ("Alexis Mac Allister", 0.07),   # Liverpool, midfield runner
        ("Enzo Fernández", 0.05),        # Chelsea, set pieces
        ("Nicolás González", 0.04),      # Fiorentina winger
        ("other", 0.06),
    ],
    "France": [
        ("Kylian Mbappé", 0.38),         # Superstar, penalty taker, Golden Boot favorite
        ("Randal Kolo Muani", 0.16),     # PSG striker
        ("Marcus Thuram", 0.14),         # Inter Milan, good form
        ("Ousmane Dembélé", 0.10),       # PSG winger
        ("Kingsley Coman", 0.07),        # Bayern winger
        ("Eduardo Camavinga", 0.04),     # Real Madrid midfield
        ("Aurélien Tchouaméni", 0.04),   # Real Madrid midfield, long shots
        ("Antoine Griezmann", 0.04),     # Back from ankle surgery (sub role)
        ("other", 0.03),
    ],
    "Brazil": [
        ("Vinícius Jr", 0.26),           # Thigh injury, may miss first match
        ("Richarlison", 0.20),           # Main striker (when fit)
        ("Raphinha", 0.16),              # Barcelona winger
        ("Rodrygo", 0.12),              # Real Madrid
        ("Neymar", 0.10),               # ACL recovery, sub role
        ("Endrick", 0.06),              # Real Madrid, super-sub
        ("Lucas Paquetá", 0.05),        # West Ham midfielder
        ("other", 0.05),
    ],
    "England": [
        ("Harry Kane", 0.34),            # Bayern, penalty taker, Golden Boot contender
        ("Jude Bellingham", 0.18),       # Real Madrid, Ballon d'Or contender
        ("Bukayo Saka", 0.14),           # Arsenal winger
        ("Phil Foden", 0.10),           # Man City
        ("Cole Palmer", 0.08),          # Chelsea, penalty taker (backup)
        ("Declan Rice", 0.04),          # Arsenal, set pieces
        ("Marcus Rashford", 0.06),      # Man United
        ("other", 0.06),
    ],
    "Germany": [
        ("Kai Havertz", 0.20),           # Arsenal, versatile forward
        ("Niclas Füllkrug", 0.18),       # Dortmund, poacher
        ("Jamal Musiala", 0.16),         # Bayern, dribbling wizard
        ("Florian Wirtz", 0.14),         # Leverkusen, playmaker
        ("Serge Gnabry", 0.08),         # Bayern winger
        ("Ilkay Gündoğan", 0.08),       # Barcelona, penalty taker
        ("Leroy Sané", 0.08),           # Bayern winger
        ("Joshua Kimmich", 0.03),       # Bayern, set pieces
        ("other", 0.05),
    ],
    "Spain": [
        ("Álvaro Morata", 0.22),         # Atlético Madrid, main striker
        ("Mikel Oyarzabal", 0.16),       # Real Sociedad, penalty taker
        ("Joselu", 0.14),               # Real Madrid supersub
        ("Nico Williams", 0.13),        # Athletic Club, pace
        ("Dani Olmo", 0.10),            # Leipzig playmaker
        ("Ferrán Torres", 0.08),        # Barcelona winger
        ("Fabián Ruiz", 0.06),          # PSG midfield
        ("Dani Carvajal", 0.03),        # Right back, rare goals
        ("other", 0.08),
    ],
    "Portugal": [
        ("Cristiano Ronaldo", 0.26),     # 40yo, but still scoring, penalty taker
        ("Rafael Leão", 0.18),          # AC Milan winger
        ("Gonçalo Ramos", 0.16),        # PSG striker
        ("Bruno Fernandes", 0.10),      # Man United midfielder
        ("Bernardo Silva", 0.08),       # Man City
        ("João Félix", 0.07),           # Chelsea/Barcelona
        ("Diogo Jota", 0.07),           # Liverpool
        ("João Cancelo", 0.03),         # Full back
        ("other", 0.05),
    ],
    "Netherlands": [
        ("Memphis Depay", 0.24),         # Penalty taker, main forward
        ("Cody Gakpo", 0.20),           # Liverpool, 2022 WC breakout
        ("Wout Weghorst", 0.14),        # Target man
        ("Xavi Simons", 0.12),          # Leipzig playmaker
        ("Donyell Malen", 0.08),        # Dortmund
        ("Frenkie de Jong", 0.06),      # Barcelona midfield
        ("Denzel Dumfries", 0.05),      # Right wing-back
        ("Jeremie Frimpong", 0.05),     # Leverkusen wing-back
        ("other", 0.06),
    ],
    "Belgium": [
        ("Romelu Lukaku", 0.26),         # All-time top scorer, main striker
        ("Kevin De Bruyne", 0.14),      # Man City, playmaker
        ("Leandro Trossard", 0.12),     # Arsenal winger
        ("Loïs Openda", 0.12),          # Leipzig striker
        ("Jérémy Doku", 0.08),          # Man City winger
        ("Youri Tielemans", 0.07),      # Aston Villa midfield
        ("Michy Batshuayi", 0.10),      # Supersub striker
        ("Amadou Onana", 0.04),         # Midfield
        ("other", 0.07),
    ],
    "Croatia": [
        ("Andrej Kramarić", 0.20),       # Hoffenheim striker
        ("Luka Modrić", 0.10),          # 40yo, set pieces
        ("Bruno Petković", 0.18),       # Dinamo Zagreb striker
        ("Ivan Perišić", 0.12),         # Veteran winger
        ("Mislav Oršić", 0.10),         # Dinamo Zagreb
        ("Mateo Kovačić", 0.06),        # Man City midfield
        ("Mario Pašalić", 0.08),        # Atalanta
        ("other", 0.16),
    ],
    "Uruguay": [
        ("Darwin Núñez", 0.28),          # Liverpool, main striker
        ("Federico Valverde", 0.16),    # Real Madrid, midfield
        ("Facundo Pellistri", 0.10),    # Man United winger
        ("Maximiliano Gómez", 0.12),    # Valencia striker
        ("Giorgian de Arrascaeta", 0.10),# Flamengo playmaker
        ("Rodrigo Bentancur", 0.06),    # Spurs midfield
        ("Ronald Araújo", 0.04),        # CB, set pieces
        ("other", 0.14),
    ],
    "Colombia": [
        ("Luis Díaz", 0.28),             # Liverpool winger
        ("Rafael Santos Borré", 0.18),  # Internacional striker
        ("James Rodríguez", 0.12),      # Veteran playmaker
        ("Jhon Arias", 0.10),           # Fluminense
        ("Jhon Durán", 0.12),           # Aston Villa
        ("Juan Cuadrado", 0.06),        # Veteran winger
        ("Yerson Candelo", 0.04),       # Full back
        ("other", 0.10),
    ],
    "Morocco": [
        ("Youssef En-Nesyri", 0.26),     # Sevilla, 2022 WC star
        ("Sofiane Boufal", 0.12),       # Angers
        ("Achraf Hakimi", 0.10),        # PSG, wing-back
        ("Hakim Ziyech", 0.12),         # Chelsea
        ("Ayoub El Kaabi", 0.14),       # Olympiacos
        ("Azzedine Ounahi", 0.08),      # Marseille
        ("Selim Amallah", 0.06),        # Valencia
        ("other", 0.12),
    ],
    "Japan": [
        ("Kaoru Mitoma", 0.22),          # Brighton winger
        ("Takefusa Kubo", 0.18),        # Real Sociedad
        ("Ayase Ueda", 0.16),           # Feyenoord striker
        ("Daichi Kamada", 0.10),        # Crystal Palace
        ("Ritsu Dōan", 0.10),           # Freiburg
        ("Hiroki Itō", 0.08),           # Stuttgart
        ("Junya Itō", 0.06),            # Reims
        ("other", 0.10),
    ],
    "South Korea": [
        ("Son Heung-min", 0.30),         # Spurs captain, penalty taker
        ("Hwang Hee-chan", 0.16),       # Wolves
        ("Lee Kang-in", 0.12),          # PSG playmaker
        ("Cho Gue-sung", 0.14),         # Jeonbuk striker
        ("Hwang In-beom", 0.08),        # Zvezda midfield
        ("Kim Min-jae", 0.04),          # Bayern CB, set pieces
        ("Jung Woo-young", 0.06),       # Stuttgart
        ("other", 0.10),
    ],
    "USA": [
        ("Christian Pulisic", 0.22),     # AC Milan, captain
        ("Folarin Balogun", 0.18),      # Monaco striker
        ("Weston McKennie", 0.10),      # Juventus midfield
        ("Gio Reyna", 0.12),            # Dortmund
        ("Tim Weah", 0.10),             # Juventus winger
        ("Ricardo Pepi", 0.10),         # PSV striker
        ("Brenden Aaronson", 0.06),     # Leeds
        ("other", 0.12),
    ],
    "Canada": [
        ("Jonathan David", 0.28),        # Lille striker, prolific
        ("Alphonso Davies", 0.16),      # Bayern wing-back
        ("Cyle Larin", 0.18),           # Mallorca striker
        ("Tajon Buchanan", 0.10),       # Inter winger
        ("Stephen Eustáquio", 0.06),    # Porto midfield
        ("Ismaël Koné", 0.06),          # Marseille
        ("Richie Laryea", 0.04),        # Toronto
        ("other", 0.12),
    ],
    "Mexico": [
        ("Raúl Jiménez", 0.20),          # Fulham, penalty taker
        ("Hirving Lozano", 0.16),       # PSV winger
        ("Santiago Giménez", 0.18),     # Feyenoord striker
        ("Orbelín Pineda", 0.08),       # AEK Athens
        ("Uriel Antuna", 0.08),         # Cruz Azul
        ("Érick Gutiérrez", 0.06),      # PSV
        ("Julián Quiñones", 0.10),      # América
        ("other", 0.14),
    ],
    "Sweden": [
        ("Alexander Isak", 0.26),        # Newcastle, world-class talent
        ("Viktor Gyökeres", 0.24),      # Sporting CP, prolific goalscorer
        ("Dejan Kulusevski", 0.12),     # Spurs winger
        ("Emil Forsberg", 0.10),        # RB Leipzig, set pieces
        ("Anthony Elanga", 0.08),       # Nottingham Forest
        ("Jens Cajuste", 0.04),         # Midfield
        ("Hugo Larsson", 0.04),         # Frankfurt midfield
        ("other", 0.12),
    ],
    "Norway": [
        ("Erling Haaland", 0.40),        # Man City, single-season record scorer
        ("Martin Ødegaard", 0.14),      # Arsenal captain
        ("Alexander Sørloth", 0.16),    # Atlético Madrid striker
        ("Jørgen Strand Larsen", 0.08), # Wolves
        ("Morten Thorsby", 0.04),       # Midfield
        ("Jesper Daland", 0.03),        # CB, set pieces
        ("other", 0.15),
    ],
    "Switzerland": [
        ("Breel Embolo", 0.24),          # Monaco striker
        ("Xherdan Shaqiri", 0.12),      # Chicago Fire, set pieces
        ("Granit Xhaka", 0.08),         # Leverkusen
        ("Ruben Vargas", 0.10),         # Augsburg
        ("Noah Okafor", 0.12),          # Milan
        ("Manuel Akanji", 0.04),        # Man City CB
        ("Remo Freuler", 0.04),         # Bologna
        ("other", 0.26),
    ],
    "Denmark": [
        ("Rasmus Højlund", 0.26),        # Man United striker
        ("Christian Eriksen", 0.14),    # Man United, set pieces
        ("Pione Sisto", 0.10),          # Midtjylland
        ("Mikkel Damsgaard", 0.08),     # Brentford
        ("Yussuf Poulsen", 0.10),       # RB Leipzig
        ("Joachim Andersen", 0.04),     # CB, set pieces
        ("other", 0.28),
    ],
    "Senegal": [
        ("Sadio Mané", 0.28),            # Al Nassr, captain, penalty taker
        ("Boulaye Dia", 0.18),          # Salernitana striker
        ("Ismaïla Sarr", 0.12),         # Crystal Palace winger
        ("Nicolas Jackson", 0.14),      # Chelsea striker
        ("Pape Matar Sarr", 0.06),      # Spurs midfield
        ("Krépin Diatta", 0.06),        # Monaco
        ("other", 0.16),
    ],
    "Egypt": [
        ("Mohamed Salah", 0.34),         # Liverpool, penalty taker
        ("Omar Marmoush", 0.16),        # Frankfurt
        ("Mostafa Mohamed", 0.14),      # Nantes striker
        ("Trézéguet", 0.08),           # Trabzonspor
        ("Mohamed Elneny", 0.04),       # Arsenal
        ("Hamdi Fathi", 0.04),          # Midfield
        ("other", 0.20),
    ],
    "Scotland": [
        ("Scott McTominay", 0.18),       # Man United midfield
        ("John McGinn", 0.16),          # Aston Villa midfield
        ("Che Adams", 0.16),            # Southampton striker
        ("Lyndon Dykes", 0.12),         # QPR striker
        ("Ryan Christie", 0.08),        # Bournemouth
        ("Andy Robertson", 0.06),       # Liverpool full back
        ("Billy Gilmour", 0.04),        # Brighton
        ("other", 0.20),
    ],
    "Turkey": [
        ("Hakan Çalhanoğlu", 0.22),      # Inter, set pieces, penalty taker
        ("Cenk Tosun", 0.16),           # Fenerbahçe striker
        ("Kerem Aktürkoğlu", 0.12),     # Benfica winger
        ("Yusuf Yazıcı", 0.10),         # Lille
        ("Arda Güler", 0.12),           # Real Madrid
        ("Abdülkadir Ömür", 0.06),      # Hull City
        ("Çağlar Söyüncü", 0.04),       # CB, set pieces
        ("other", 0.18),
    ],
    "Ecuador": [
        ("Enner Valencia", 0.26),        # Main striker, captain
        ("Gonzalo Plata", 0.12),        # Flamengo winger
        ("Moisés Caicedo", 0.08),       # Chelsea midfield
        ("Kendry Páez", 0.12),          # Independiente, wonderkid
        ("Jeremy Sarmiento", 0.10),     # Brighton winger
        ("Alan Minda", 0.08),           # Cercle Brugge
        ("other", 0.24),
    ],
    "Iran": [
        ("Mehdi Taremi", 0.26),          # Inter, main striker
        ("Sardar Azmoun", 0.20),         # Roma striker
        ("Alireza Jahanbakhsh", 0.10),  # Feyenoord winger
        ("Saman Ghoddos", 0.08),        # Midfield
        ("Milad Mohammadi", 0.04),      # Full back
        ("Ali Gholizadeh", 0.08),       # Lech Poznań
        ("other", 0.24),
    ],
    "Czech Republic": [
        ("Patrik Schick", 0.26),         # Leverkusen striker
        ("Tomáš Souček", 0.14),         # West Ham, set pieces
        ("Adam Hložek", 0.12),          # Leverkusen
        ("Václav Černý", 0.10),         # Wolfsburg
        ("Antonín Barák", 0.08),        # Midfield
        ("Ladislav Krejčí", 0.06),      # Sparta Prague
        ("other", 0.24),
    ],
    "Austria": [
        ("Marko Arnautović", 0.22),      # Inter, main striker
        ("Christoph Baumgartner", 0.16),# RB Leipzig
        ("Marcel Sabitzer", 0.12),      # Dortmund midfield
        ("Konrad Laimer", 0.08),        # Bayern midfield
        ("Michael Gregoritsch", 0.12),  # Freiburg striker
        ("Xaver Schlager", 0.06),       # Midfield
        ("other", 0.24),
    ],
    "Ghana": [
        ("Mohammed Kudus", 0.22),        # West Ham, star player
        ("Iñaki Williams", 0.20),       # Athletic Club
        ("Jordan Ayew", 0.14),          # Palace
        ("André Ayew", 0.10),           # Le Havre
        ("Ransford-Yeboah Königsdörffer", 0.06), # Hamburg
        ("Thomas Partey", 0.06),        # Arsenal midfield
        ("other", 0.22),
    ],
    "South Africa": [
        ("Percy Tau", 0.20),             # Al Ahly, star player
        ("Lyle Foster", 0.16),          # Burnley striker
        ("Themba Zwane", 0.10),         # Mamelodi Sundowns
        ("Mothobi Mvala", 0.08),        # Midfield
        ("Evidence Makgopa", 0.10),     # Orlando Pirates
        ("Teboho Mokoena", 0.06),       # Midfield
        ("other", 0.30),
    ],
    "Paraguay": [
        ("Miguel Almirón", 0.18),        # Newcastle playmaker
        ("Antonio Sanabria", 0.16),     # Torino striker
        ("Derlis González", 0.10),      # Olimpia
        ("Ángel Romero", 0.10),         # Corinthians
        ("Julio Enciso", 0.12),         # Brighton
        ("Óscar Cardozo", 0.08),        # Libertad
        ("other", 0.26),
    ],
    "Algeria": [
        ("Riyad Mahrez", 0.22),          # Al Ahli, captain
        ("Islam Slimani", 0.14),        # Striker
        ("Said Benrahma", 0.12),        # West Ham winger
        ("Houssem Aouar", 0.08),        # Midfield
        ("Andy Delort", 0.10),          # Striker
        ("Ramy Bensebaini", 0.04),      # Dortmund FB
        ("other", 0.30),
    ],
    "Bosnia and Herzegovina": [
        ("Edin Džeko", 0.24),            # Captain, veteran striker
        ("Miralem Pjanić", 0.12),       # Sharjah, set pieces
        ("Smail Prevljak", 0.10),       # Eupen striker
        ("Luka Menalo", 0.08),          # Dinamo Zagreb
        ("Rade Krunić", 0.08),          # Midfield
        ("Anel Ahmedhodžić", 0.04),     # CB, set pieces
        ("other", 0.34),
    ],
    "Ivory Coast": [
        ("Sébastien Haller", 0.22),      # Dortmund striker
        ("Nicolas Pépé", 0.14),         # Nice winger
        ("Franck Kessié", 0.12),        # Al Ahli midfield
        ("Jérémie Boga", 0.10),         # Nice winger
        ("Maxwel Cornet", 0.08),        # West Ham
        ("Wilfried Zaha", 0.10),        # Galatasaray
        ("other", 0.24),
    ],
    "Tunisia": [
        ("Wahbi Khazri", 0.18),          # Montpellier, captain
        ("Youssef Msakni", 0.14),       # Al Arabi
        ("Anis Ben Slimane", 0.10),     # Sheffield United
        ("Elias Achouri", 0.08),        # Copenhagen
        ("Naim Sliti", 0.08),           # Al Ettifaq
        ("Aïssa Laïdouni", 0.06),       # Midfield
        ("other", 0.36),
    ],
    "Saudi Arabia": [
        ("Salem Al-Dawsari", 0.22),      # Al Hilal, 2022 hero
        ("Firas Al-Buraikan", 0.16),    # Al Ahli striker
        ("Abdullah Al-Hamdan", 0.10),   # Al Hilal
        ("Sami Al-Najei", 0.08),        # Al Nassr
        ("Salem Al-Najdi", 0.06),       # AL Hilal
        ("Mohamed Kanno", 0.06),        # Midfield
        ("other", 0.32),
    ],
    "Cape Verde": [
        ("Ryan Mendes", 0.16),           # Striker
        ("Jamiro Monteiro", 0.12),      # Midfield
        ("Garry Rodrigues", 0.10),      # Winger
        ("Jovane Cabral", 0.10),        # Sporting winger
        ("Djaniny", 0.10),              # Striker
        ("other", 0.42),
    ],
    "Haiti": [
        ("Duckens Nazon", 0.18),         # Main striker
        ("Carnejy Antoine", 0.14),      # Striker
        ("Hervé Bazile", 0.10),         # Winger
        ("Ricardo Adé", 0.06),          # CB
        ("Franzdy Pierrot", 0.08),      # Striker
        ("other", 0.44),
    ],
    "Curaçao": [
        ("Charlison Benschop", 0.16),    # Striker
        ("Leandro Bacuna", 0.12),       # Midfield
        ("Jarchinio Antonia", 0.10),    # Winger
        ("Rangelo Janga", 0.10),        # Striker
        ("Juninho Bacuna", 0.08),       # Midfield
        ("other", 0.44),
    ],
    "Iraq": [
        ("Aymen Hussein", 0.18),         # Main striker
        ("Ahmed Yasin", 0.10),          # Winger
        ("Ibrahim Bayesh", 0.10),       # Midfield
        ("Ali Ghalib", 0.06),           # Midfield
        ("Hussam Fadhel", 0.06),        # Midfield
        ("Mohammad Dawood", 0.08),      # Striker
        ("other", 0.42),
    ],
    "Jordan": [
        ("Musa Al-Taamari", 0.18),       # Montpellier, star player
        ("Ali Olwan", 0.12),            # Striker
        ("Hamza Al-Dardour", 0.10),     # Striker
        ("Mohammad Abu Zrayq", 0.08),   # Midfield
        ("Baha' Faisal", 0.08),         # Striker
        ("other", 0.44),
    ],
    "DR Congo": [
        ("Cédric Bakambu", 0.18),        # Striker
        ("Yannick Bolasie", 0.10),      # Winger
        ("Samuel Moutoussamy", 0.08),   # Midfield
        ("Grad Diangana", 0.08),        # Winger
        ("Ben Malango", 0.10),          # Striker
        ("other", 0.46),
    ],
    "Uzbekistan": [
        ("Eldor Shomurodov", 0.22),      # Roma striker
        ("Jaloliddin Masharipov", 0.12),# Midfield
        ("Oston Urunov", 0.08),         # Winger
        ("Khojiakbar Alidzhanov", 0.06),# Midfield
        ("Igor Sergeev", 0.10),         # Striker
        ("other", 0.42),
    ],
    "Qatar": [
        ("Almoez Ali", 0.20),            # Striker, 2019 Asian Cup top scorer
        ("Akram Afif", 0.16),           # Al Sadd, playmaker
        ("Hassan Al-Haydos", 0.10),     # Captain
        ("Karim Boudiaf", 0.06),        # Midfield
        ("Ismaeel Mohammad", 0.06),     # Winger
        ("other", 0.42),
    ],
    "Panama": [
        ("José Fajardo", 0.14),          # Striker
        ("Ismael Díaz", 0.12),          # Striker
        ("Édgar Bárcenas", 0.10),       # Midfield
        ("Cecilio Waterman", 0.08),     # Striker
        ("Jovani Welch", 0.06),         # Midfield
        ("other", 0.50),
    ],
    "New Zealand": [
        ("Chris Wood", 0.26),            # Nottingham Forest, penalty taker
        ("Eli Just", 0.10),             # Striker
        ("Ben Waine", 0.10),            # Striker
        ("Clayton Lewis", 0.08),        # Midfield
        ("Joe Bell", 0.06),             # Midfield
        ("other", 0.40),
    ],
    "Australia": [
        ("Mitchell Duke", 0.14),         # Striker
        ("Craig Goodwin", 0.12),        # Winger
        ("Awer Mabil", 0.10),           # Winger
        ("Riley McGree", 0.08),         # Midfield
        ("Jackson Irvine", 0.08),       # Midfield
        ("Keanu Baccus", 0.06),         # Midfield
        ("other", 0.42),
    ],
}
