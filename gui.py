import pygame

pygame.init()

HEIGHT = 480
WIDTH = 480

screen = pygame.display.set_mode((WIDTH, HEIGHT))
clock = pygame.time.Clock()

pygame.display.set_caption("Chess")

running = True

while running:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False

        if event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
            running = False

    for i in range(8):
        for j in range(8):
            color = (50, 168, 82) if (i + j) % 2 else (191, 246, 195)
                        
            pygame.draw.rect(screen, color, (i * WIDTH // 8, j * HEIGHT // 8, WIDTH // 8, HEIGHT // 8))

    pygame.display.flip()
            
    clock.tick(60)

pygame.quit()