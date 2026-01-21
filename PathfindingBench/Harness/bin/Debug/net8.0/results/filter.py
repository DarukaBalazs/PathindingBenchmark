import pandas as pd

df = pd.read_csv("results.csv", sep=';')
df = df[df["Found"] == 1]

df.to_csv("results_filtered.csv", sep=';', index=False)
print("Kész: results_filtered.csv")