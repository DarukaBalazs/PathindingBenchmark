import json
import os

SUMMARY_FILE = 'results/summary.json'

if not os.path.exists(SUMMARY_FILE):
    print("Nincs meg a summary.json!")
    exit()

with open(SUMMARY_FILE, 'r') as f:
    data = json.load(f)

print(f"{'SCENARIO':<40} | {'ALGO':<15} | {'TIME (avg)':<10} | {'MEM (avg)':<10}")
print("-" * 85)

for item in data:
    scenario = item['scenario']
    algo = item['algorithm']
    
    # Az előre kiszámolt statisztikák elérése
    time_avg = item['stats']['time']['avg']
    mem_avg = item['stats']['memory']['avg'] / 1024.0 # Konvertálás KB-ba
    
    # Csak azokat írjuk ki, ahol van releváns adat
    print(f"{scenario:<40} | {algo:<15} | {time_avg:6.2f} ms | {mem_avg:6.1f} KB")