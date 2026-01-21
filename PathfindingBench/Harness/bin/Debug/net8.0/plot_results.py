import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import os

# --- BEÁLLÍTÁSOK ---
CSV_FILE = 'results/results.csv' # Az útvonalad a CSV-hez
OUTPUT_DIR = 'results/plots'     # Ide mentjük a képeket

if not os.path.exists(OUTPUT_DIR):
    os.makedirs(OUTPUT_DIR)

# 1. ADATOK BETÖLTÉSE
print("Adatok betöltése...")
df = pd.read_csv(CSV_FILE, sep=';') # Figyelj a szeparátorra! A CsvResultWriter pontosvesszőt használhatott.

# Ellenőrzés: ha véletlenül vesszővel van elválasztva, próbáljuk újra
if df.shape[1] < 2:
    df = pd.read_csv(CSV_FILE, sep=',')

# 2. ADATTISZTÍTÁS ÉS SZÁRMAZTATOTT OSZLOPOK
# Eldobjuk a sikertelen kereséseket (ha lennének)
df = df[df['Found'] == 1]

# Kiszámoljuk az "Optimalitást" (Hányszor hosszabb a talált út, mint a légvonal?)
# Ha a távolság 0 (start==goal), akkor 1-nek vesszük, hogy ne osszunk nullával
df['Optimality'] = df.apply(lambda row: row['PathCost'] / row['LinearDistance'] if row['LinearDistance'] > 0 else 1.0, axis=1)

# Normalizált idő (ms / méter)
df['Speed_ms_per_m'] = df.apply(lambda row: row['ElapsedMs'] / row['LinearDistance'] if row['LinearDistance'] > 0 else 0, axis=1)

# Konvertáljuk a MapSize-t olvashatóbb formába
df['MapSize'] = df['MapWidth'].astype(str) + "x" + df['MapHeight'].astype(str)

# -----------------------------------------------------------
# 3. HOGYAN SZŰRJÜNK? (Példák)
# -----------------------------------------------------------

# A) Különválasztjuk a Random és Labirintus pályákat, mert összehasonlíthatatlanok
df_random = df[df['MapType'] == 'Random']
df_maze = df[df['MapType'] == 'MazeLike']

# B) Különválasztjuk a Diagonal és NoDiagonal futásokat a Randomon belül
df_random_diag = df_random[df_random['AllowDiagonal'] == True]
df_random_nodiag = df_random[df_random['AllowDiagonal'] == False]

print(f"Random (Diag) sorok: {len(df_random_diag)}")
print(f"Random (NoDiag) sorok: {len(df_random_nodiag)}")
print(f"Maze sorok: {len(df_maze)}")

# -----------------------------------------------------------
# 4. GRAFIKONOK GENERÁLÁSA
# -----------------------------------------------------------
sns.set_theme(style="whitegrid") # Szép fehér rácsos háttér

# --- GRAFIKON 1: SEBESSÉG vs TÁVOLSÁG (Random Pályák - Diagonal) ---
# Ez a legfontosabb ábra. Megmutatja, hogyan lassulnak az algoritmusok.
plt.figure(figsize=(10, 6))
sns.scatterplot(
    data=df_random_diag, 
    x='LinearDistance', 
    y='ElapsedMs', 
    hue='Algorithm', 
    style='Algorithm', 
    alpha=0.6 
)
plt.title('Algoritmusok Sebessége a Távolság függvényében (Random Map, Diagonal)')
plt.xlabel('Légvonalbeli Távolság (cella)')
plt.ylabel('Futási Idő (ms)')
plt.savefig(f'{OUTPUT_DIR}/1_Speed_vs_Distance_RandomDiag.png')
plt.close()

plt.figure(figsize=(10, 6))
g = sns.barplot(
    data=df, 
    x='MapSize', 
    y='AllocBytes', 
    hue='Algorithm',
    estimator='mean', 
    errorbar=None
)
g.set_yscale("log") 
plt.title('Átlagos Memóriahasználat (Logaritmikus skála)')
plt.ylabel('Allocated Bytes (Log)')
plt.savefig(f'{OUTPUT_DIR}/2_Memory_Usage_Log.png')
plt.close()

# --- GRAFIKON 3: EXPANSIONS (Hatékonyság) ---
# Logaritmikus skálán, mert a Dijkstra nagyságrendekkel több lehet
plt.figure(figsize=(10, 6))
g = sns.scatterplot(
    data=df_random_diag,
    x='LinearDistance',
    y='Expansions',
    hue='Algorithm',
    alpha=0.7
)
g.set_yscale("log") 
plt.title('Kiterjesztett Csomópontok (Logaritmikus skála)')
plt.ylabel('Expansions (Log)')
plt.savefig(f'{OUTPUT_DIR}/3_Expansions_LogScale.png')
plt.close()

# --- GRAFIKON 4: OPTIMALITÁS (Box Plot) ---
# Mennyire tér el az egyenestől az út?
plt.figure(figsize=(8, 6))
sns.boxplot(
    data=df_random_diag,
    x='Algorithm',
    y='Optimality'
)
plt.axhline(1.0, color='red', linestyle='--', label='Tökéletes (Légvonal)')
plt.title('Útvonal Optimalitás (1.0 = Légvonal)')
plt.ylabel('Path Length / Linear Distance')
plt.legend()
plt.savefig(f'{OUTPUT_DIR}/4_Optimality_Boxplot.png')
plt.close()

# --- GRAFIKON 5: MAZE vs RANDOM (JPS szenvedésének bemutatása) ---
df_512 = df[df['MapWidth'] == 512]
df_compare = df_512[df_512['AllowDiagonal'] == False] 

plt.figure(figsize=(10, 6))
sns.barplot(
    data=df_compare,
    x='Algorithm',
    y='ElapsedMs',
    hue='MapType'
)
plt.title('Algoritmusok Teljesítménye: Random vs Maze (512x512, NoDiag)')
plt.savefig(f'{OUTPUT_DIR}/5_Maze_vs_Random_NoDiag.png')
plt.close()

print(f"Kész! A grafikonok a {OUTPUT_DIR} mappában vannak.")