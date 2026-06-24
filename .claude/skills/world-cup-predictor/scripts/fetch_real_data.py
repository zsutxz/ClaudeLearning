#!/usr/bin/env python3
"""
Fetch 2026 World Cup qualification match data from Wikipedia API.
Pipeline: WSL → PowerShell → Windows .NET HttpClient → Wikipedia API → JSON → Parse

Usage:
    python3 scripts/fetch_real_data.py              # Print all confederation stats
    python3 scripts/fetch_real_data.py --save        # Save to references/ dir

This is the full multi-confederation pipeline developed 2026-05-23.
Covers: CONMEBOL, UEFA, CAF, AFC, CONCACAF, OFC = 800+ matches, 44/48 WC teams.
"""
import json, re, os, sys

# === CONFIG ===
TEMP = os.path.join(os.getcwd(), "docs", "world-cup-predictor", "_temp")
os.makedirs(TEMP, exist_ok=True)
SKILL_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# FIFA 3-letter code → standard team name mapping
FIFA_CODE = {
    'ARG':'Argentina','BRA':'Brazil','URU':'Uruguay','COL':'Colombia','ECU':'Ecuador','PAR':'Paraguay',
    'GER':'Germany','FRA':'France','ESP':'Spain','ENG':'England','NED':'Netherlands','POR':'Portugal',
    'BEL':'Belgium','SUI':'Switzerland','CRO':'Croatia','SWE':'Sweden','AUT':'Austria','TUR':'Turkey',
    'SCO':'Scotland','NOR':'Norway','CZE':'Czech Republic','BIH':'Bosnia and Herzegovina',
    'USA':'United States','MEX':'Mexico','CAN':'Canada','PAN':'Panama','HAI':'Haiti','CUR':'Curaçao',
    'MAR':'Morocco','SEN':'Senegal','EGY':'Egypt','TUN':'Tunisia','CIV':'Ivory Coast','ALG':'Algeria',
    'GHA':'Ghana','CPV':'Cape Verde','COD':'DR Congo','RSA':'South Africa','QAT':'Qatar',
    'JPN':'Japan','KOR':'South Korea','AUS':'Australia','IRN':'Iran','KSA':'Saudi Arabia','IRQ':'Iraq',
    'UZB':'Uzbekistan','JOR':'Jordan','NZL':'New Zealand',
    # Extended for AFC/CONCACAF/CAF qualifiers
    'IND':'India','AFG':'Afghanistan','BHR':'Bahrain','BAN':'Bangladesh','BHU':'Bhutan','BRU':'Brunei',
    'CAM':'Cambodia','CHN':'China','TPE':'Chinese Taipei','GUM':'Guam','HKG':'Hong Kong','IDN':'Indonesia',
    'PRK':'North Korea','LAO':'Laos','LIB':'Lebanon','MAC':'Macau','MAS':'Malaysia','MDV':'Maldives',
    'MNG':'Mongolia','MYA':'Myanmar','NEP':'Nepal','OMA':'Oman','PAK':'Pakistan','PLE':'Palestine',
    'PHI':'Philippines','SIN':'Singapore','SRI':'Sri Lanka','SYR':'Syria','TJK':'Tajikistan',
    'THA':'Thailand','TLS':'East Timor','TKM':'Turkmenistan','UAE':'United Arab Emirates','VIE':'Vietnam',
    'YEM':'Yemen','KUW':'Kuwait','KGZ':'Kyrgyzstan',
    'BFA':'Burkina Faso','CMR':'Cameroon','NGA':'Nigeria','MLI':'Mali','ZAM':'Zambia','GAB':'Gabon',
    'EQG':'Equatorial Guinea','BEN':'Benin','ANG':'Angola','GUI':'Guinea','MAD':'Madagascar','COM':'Comoros',
    'MOZ':'Mozambique','TOG':'Togo','SUD':'Sudan','NIG':'Niger','RWA':'Rwanda','ETH':'Ethiopia',
    'TAN':'Tanzania','UGA':'Uganda','ZIM':'Zimbabwe','MWI':'Malawi','CTA':'Central African Republic',
    'LBY':'Libya','SLE':'Sierra Leone','BOT':'Botswana','MTN':'Mauritania','BDI':'Burundi','SSD':'South Sudan',
    'MRI':'Mauritius','SWZ':'Eswatini','LES':'Lesotho','SEY':'Seychelles','GAM':'Gambia','STP':'São Tomé',
    'SOM':'Somalia','KEN':'Kenya','TCH':'Chad','LBR':'Liberia','DJI':'Djibouti','LBN':'Lebanon',
    'CRC':'Costa Rica','HON':'Honduras','SLV':'El Salvador','JAM':'Jamaica','TRI':'Trinidad and Tobago',
    'GUY':'Guyana','NCA':'Nicaragua','BRB':'Barbados','DMA':'Dominica','LCA':'Saint Lucia','GRN':'Grenada',
    'BER':'Bermuda','BLZ':'Belize','DOM':'Dominican Republic','SKN':'Saint Kitts and Nevis',
    'VIN':'Saint Vincent and the Grenadines','ARU':'Aruba','PUR':'Puerto Rico','CUB':'Cuba',
    'CAY':'Cayman Islands','BAH':'Bahamas','AIA':'Anguilla','MSR':'Montserrat','VIR':'US Virgin Islands',
    'VGB':'British Virgin Islands','TCA':'Turks and Caicos Islands',
    'COK':'Cook Islands','SAM':'Samoa','TGA':'Tonga','ASA':'American Samoa','SOL':'Solomon Islands',
    'NCL':'New Caledonia','TAH':'Tahiti','FIJ':'Fiji','VAN':'Vanuatu','PNG':'Papua New Guinea',
}

# Confederation per team
CONF = {
    'Argentina':'CONMEBOL','Brazil':'CONMEBOL','Uruguay':'CONMEBOL','Colombia':'CONMEBOL',
    'Ecuador':'CONMEBOL','Paraguay':'CONMEBOL',
    'United States':'CONCACAF','Mexico':'CONCACAF','Canada':'CONCACAF','Panama':'CONCACAF',
    'Haiti':'CONCACAF','Curaçao':'CONCACAF',
    'Japan':'AFC','South Korea':'AFC','Australia':'AFC','Iran':'AFC','Saudi Arabia':'AFC',
    'Iraq':'AFC','Uzbekistan':'AFC','Jordan':'AFC','Qatar':'AFC',
    'Morocco':'CAF','Senegal':'CAF','Egypt':'CAF','Tunisia':'CAF','Algeria':'CAF',
    'Ivory Coast':'CAF','Ghana':'CAF','Cape Verde':'CAF','DR Congo':'CAF','South Africa':'CAF',
    'New Zealand':'OFC',
}

CONF_FACTOR = {'UEFA':1.0, 'CONMEBOL':0.95, 'CAF':0.85, 'AFC':0.80, 'CONCACAF':0.75, 'OFC':0.40}


def load_wt(fname):
    """Load wikitext from saved JSON file."""
    path = os.path.join(TEMP, fname)
    if not os.path.exists(path):
        print(f"  MISSING: {fname}"); return None
    with open(path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    return data['parse']['wikitext']['*']

def load_html(fname):
    """Load rendered HTML from saved JSON file."""
    path = os.path.join(TEMP, fname)
    if not os.path.exists(path):
        print(f"  MISSING: {fname}"); return None
    with open(path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    return data['parse']['text']['*']


def parse_all():
    """Parse all confederations, return list of (team, gf, ga, team2, source)"""
    all_matches = []
    
    # CONMEBOL + OFC: wikitext football box
    for fname, src in [('conmebol.json','CONMEBOL'), ('ofc.json','OFC')]:
        wt = load_wt(fname)
        if wt:
            for m in re.finditer(r'\|team1\s*=\s*\{\{fb-rt\|([^}]+)\}\}\s*\n.*?\|score\s*=\s*(\d+)[\u2013-](\d+)\s*\n.*?\|team2\s*=\s*\{\{fb\|([^}]+)\}\}', wt, re.DOTALL):
                t1, s1, s2, t2 = m.group(1), int(m.group(2)), int(m.group(3)), m.group(4)
                if t1 in FIFA_CODE and t2 in FIFA_CODE:
                    all_matches.append((FIFA_CODE[t1], s1, s2, FIFA_CODE[t2], src))
    
    # UEFA + CAF: template sports table |match_XXX_YYY=
    for fname, src in [('uefa_template.json','UEFA'), ('caf_template.json','CAF')]:
        wt = load_wt(fname)
        if wt:
            for m in re.finditer(r'\|match_([A-Z]+)_([A-Z]+)\s*=\s*\[\[[^\]]*\|(\d+)[\u2013-](\d+)\]\]', wt):
                t1, t2 = m.group(1), m.group(2)
                if t1 in FIFA_CODE and t2 in FIFA_CODE:
                    all_matches.append((FIFA_CODE[t1], int(m.group(3)), int(m.group(4)), FIFA_CODE[t2], src))
    
    # AFC + CONCACAF: rendered HTML anchor links
    for src, fnames in [
        ('AFC', ['2026_FIFA_World_Cup_qualification__AFC_second_round.json',
                 '2026_FIFA_World_Cup_qualification__AFC_third_round.json',
                 '2026_FIFA_World_Cup_qualification__AFC_fourth_round.json']),
        ('CONCACAF', ['2026_FIFA_World_Cup_qualification__CONCACAF_second_round.json',
                      '2026_FIFA_World_Cup_qualification__CONCACAF_third_round.json']),
    ]:
        for fname in fnames:
            html = load_html(fname)
            if html:
                for m in re.finditer(r'href="#([A-Z]+)_v_([A-Z]+)"[^>]*>\s*(\d+)[\u2013-](\d+)\s*</a>', html):
                    t1, t2 = m.group(1), m.group(2)
                    if t1 in FIFA_CODE and t2 in FIFA_CODE:
                        all_matches.append((FIFA_CODE[t1], int(m.group(3)), int(m.group(4)), FIFA_CODE[t2], src))
    
    return all_matches


def compute_stats(matches):
    """Compute per-team goals for/against per match."""
    stats = {}
    for t1, s1, s2, t2, src in matches:
        for team, gf, ga in [(t1, s1, s2), (t2, s2, s1)]:
            if team not in stats: stats[team] = {'gf':0, 'ga':0, 'mp':0}
            stats[team]['gf'] += gf; stats[team]['ga'] += ga; stats[team]['mp'] += 1
    return stats


def normalize_with_conf(stats):
    """Apply confederation normalization and return 40-95 scaled ratings."""
    # Apply conf factor
    adj = {}
    for team, s in stats.items():
        conf = CONF.get(team, 'UEFA')
        factor = CONF_FACTOR.get(conf, 1.0)
        adj[team] = {'gf_adj': s['gf']/s['mp']*factor, 'ga_adj': s['ga']/s['mp']/factor, 'mp': s['mp']}
    
    max_gf = max(v['gf_adj'] for v in adj.values()) if adj else 4.0
    max_ga = max(v['ga_adj'] for v in adj.values()) if adj else 3.0
    
    ratings = {}
    for team, v in adj.items():
        atk = round(40 + min(55, (v['gf_adj']/max_gf)*55))
        dfn = round(95 - min(55, (v['ga_adj']/max_ga)*55))
        # Bayesian shrinkage for small samples
        weight = min(0.8, v['mp']/18)
        if weight < 0.8:
            atk = round(atk*weight + 55*(1-weight))
            dfn = round(dfn*weight + 65*(1-weight))
        ratings[team] = (atk, dfn, v['mp'], v['gf_adj'], v['ga_adj'])
    return ratings


def main():
    print("=" * 60)
    print("2026 WC Qualification — Wikipedia Data Pipeline")
    print("=" * 60)
    
    matches = parse_all()
    print(f"\nTotal matches: {len(matches)}")
    
    stats = compute_stats(matches)
    ratings = normalize_with_conf(stats)
    
    # Print WC48 teams sorted by rating
    wc48 = ['Argentina','France','Spain','England','Brazil','Netherlands','Portugal','Belgium',
            'Germany','Croatia','Uruguay','Switzerland','Colombia','Mexico','Japan','Morocco',
            'United States','Senegal','Sweden','Iran','South Korea','Australia','Austria','Turkey',
            'Ecuador','Egypt','Ghana','Norway','Paraguay','Canada','Scotland','Czech Republic',
            'South Africa','Cape Verde','Saudi Arabia','Iraq','Algeria','Tunisia','Ivory Coast',
            'DR Congo','Uzbekistan','Bosnia and Herzegovina','Jordan','New Zealand','Panama',
            'Qatar','Haiti','Curaçao']
    
    print(f"\n{'Team':25s} {'Conf':>6s} {'MP':>3s} {'atk':>4s} {'dfn':>4s} {'GF/M':>6s} {'GA/M':>6s}")
    print("-" * 60)
    for team in sorted(wc48):
        if team in ratings:
            a, d, mp, gf, ga = ratings[team]
            conf = CONF.get(team, '?')
            print(f"{team:25s} {conf:>6s} {mp:3d} {a:4d} {d:4d} {gf:6.2f} {ga:6.2f}")
        else:
            print(f"{team:25s} {'?':>6s} {'?':>3s} {'?':>4s} {'?':>4s} {'?':>6s} {'?':>6s}")

    # Save if --save flag
    if '--save' in sys.argv:
        out_path = os.path.join(SKILL_DIR, 'references', 'wc-qualifier-stats.json')
        with open(out_path, 'w') as f:
            json.dump(ratings, f, indent=2)
        print(f"\nSaved to {out_path}")


if __name__ == '__main__':
    main()
