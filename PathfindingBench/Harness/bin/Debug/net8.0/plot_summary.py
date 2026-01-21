import json
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import os
import re

# --- BEÁLLÍTÁSOK ---
JSON_FILE = 'results/summary.json'
OUTPUT_DIR = 'results/summary_plots'

if not os.path.exists(OUTPUT_DIR):
    os.makedirs(OUTPUT_DIR)

# 1. JSON BETÖLTÉSE ÉS FELDOLGOZÁSA
print("Summary JSON betöltése...")
with open(JSON_FILE, 'r') as f:
    data = json.load(f)

# Átalakítjuk a JSON-t egy lapos táblázattá (DataFrame)
rows = []
for entry in data:
    scen_name = entry['scenario']
    algo = entry['algorithm']
    stats = entry['stats']
    
    # --- STRING PARSING OKOSSÁG ---
    # A név formátuma kb: "0_100x100_dens0.10_corner_NoDiag"
    # Reguláris kifejezéssel kiszedjük az adatokat
    # Keresünk valamit, ami szám+"x"+szám (pl. 100x100)
    size_match = re.search(r'(\d+)x(\d+)', scen_name)
    width = int(size_match.group(1)) if size_match else 0
    
    # Megnézzük, hogy Random vagy Maze
    map_type = "Maze" if "Maze" in scen_name else "Random"
    
    # Megnézzük, hogy Diag vagy NoDiag
    diag_mode = "Diagonal" if "_Diag" in scen_name else "NoDiagonal"
    if "NoDiag" in scen_name: diag_mode = "NoDiagonal" # Biztosra megyünk
    
    # Adatok összegyűjtése
    rows.append({
        'Algorithm': algo,
        'MapSize': width, # Elég a szélesség, mert négyzetes
        'MapType': map_type,
        'Diagonal': diag_mode,
        'Time_Avg': stats['time']['avg'],
        'Time_P95': stats['time']['p95'], # A legrosszabb esetek (stabilitás)
        'Memory_Avg_KB': stats['memory']['avg'] / 1024.0 # KB-ra váltva
    })

df = pd.DataFrame(rows)

# 2. VIZUALIZÁCIÓ
sns.set_theme(style="whitegrid")

# --- A) SKÁLÁZÓDÁS (Vonaldiagram) ---
# Hogyan nő az átlagos idő a mérettel? (Csak Random + Diagonal esetre)
df_scaling = df[(df['MapType'] == 'Random') & (df['Diagonal'] == 'Diagonal')]

plt.figure(figsize=(10, 6))
sns.lineplot(
    data=df_scaling,
    x='MapSize',
    y='Time_Avg',
    hue='Algorithm',
    style='Algorithm',
    markers=True,
    dashes=False,
    linewidth=2.5,
    markersize=10
)
plt.title('Skálázódás: Átlagos Futási Idő a Méret függvényében (Random Map)')
plt.xlabel('Pálya Mérete (NxN)')
plt.ylabel('Átlagos Idő (ms)')
# X tengelyen csak a létező méretek legyenek
plt.xticks(df_scaling['MapSize'].unique()) 
plt.savefig(f'{OUTPUT_DIR}/S1_Scalability_Time.png')
plt.close()

# --- B) STABILITÁS (Avg vs P95) ---
# Mennyivel rosszabb a "rossz eset" (P95) mint az átlag?
# Ezt egy csoportosított oszlopdiagrammal mutatjuk meg a legnagyobb pályán (1024)
df_stability = df[(df['MapSize'] == 1024) & (df['MapType'] == 'Random') & (df['Diagonal'] == 'Diagonal')]

# Átalakítjuk "Long format"-ra a Seabornnak (Avg és P95 külön sorok legyenek)
df_melted = df_stability.melt(
    id_vars=['Algorithm'], 
    value_vars=['Time_Avg', 'Time_P95'], 
    var_name='Metric', 
    value_name='TimeMs'
)

plt.figure(figsize=(10, 6))
sns.barplot(
    data=df_melted,
    x='Algorithm',
    y='TimeMs',
    hue='Metric',
    palette='muted'
)
plt.title('Stabilitás: Átlag vs. P95 (1024x1024 Random Map)')
plt.ylabel('Idő (ms)')
plt.savefig(f'{OUTPUT_DIR}/S2_Stability_Avg_vs_P95.png')
plt.close()

# --- C) MEMÓRIA FOGYASZTÁS (Minden méreten) ---
# Itt látszik majd, ha valamelyik algoritmus "elszáll" nagy pályán
plt.figure(figsize=(10, 6))
sns.barplot(
    data=df[df['MapType'] == 'Random'],
    x='MapSize',
    y='Memory_Avg_KB',
    hue='Algorithm'
)
plt.title('Memóriahasználat Skálázódása')
plt.ylabel('Allocated Memory (KB)')
plt.savefig(f'{OUTPUT_DIR}/S3_Memory_Usage.png')
plt.close()

# --- D) MAZE vs RANDOM (NoDiagonal) ---
# Itt csak a Dijkstra és Weighted A* versenyez (meg JPS ha bekerült, de NoDiagban korlátozott)
# Fixáljuk a méretet 512-re
df_comp = df[(df['MapSize'] == 512) & (df['Diagonal'] == 'NoDiagonal')]

plt.figure(figsize=(10, 6))
sns.barplot(
    data=df_comp,
    x='Algorithm',
    y='Time_Avg',
    hue='MapType'
)
plt.title('Terep hatása: Random vs Maze (512x512, No Diagonal)')
plt.ylabel('Átlagos Idő (ms)')
plt.savefig(f'{OUTPUT_DIR}/S4_Terrain_Impact.png')
plt.close()

print(f"Kész! A summary grafikonok a {OUTPUT_DIR} mappában vannak.")